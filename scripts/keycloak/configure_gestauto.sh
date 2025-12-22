#!/usr/bin/env bash
set -euo pipefail

# Idempotent Keycloak configuration for GestAuto.
# - Creates realm, realm roles, test users
# - Creates API clients (resource servers)
# - Creates a dedicated token-minting client for dev/tests
# - Adds protocol mappers:
#   - realm roles -> custom claim "roles" (multivalued)
#   - audience -> include API client IDs in aud

require() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "ERROR: missing dependency: $1" >&2
    exit 1
  }
}

require curl
require jq

KEYCLOAK_BASE_URL="${KEYCLOAK_BASE_URL:-http://localhost:8080}"
KEYCLOAK_ADMIN_REALM="${KEYCLOAK_ADMIN_REALM:-master}"
KEYCLOAK_ADMIN_USER="${KEYCLOAK_ADMIN_USER:?set KEYCLOAK_ADMIN_USER}"
KEYCLOAK_ADMIN_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:?set KEYCLOAK_ADMIN_PASSWORD}"

GESTAUTO_REALM="${GESTAUTO_REALM:-gestauto}"

# Keycloak User Profile defaults often require email/firstName/lastName for "user" role.
# Ensure our test users satisfy those requirements so password grant works in local/dev setups.
GESTAUTO_TEST_USERS_EMAIL_DOMAIN="${GESTAUTO_TEST_USERS_EMAIL_DOMAIN:-tasso.local}"

# Workaround for local token-minting scripts: in some setups, the default "Direct Grant - Conditional OTP"
# subflow can break password grant even when users don't have TOTP configured.
# Disable it by default for local/dev unless explicitly opted out.
GESTAUTO_DIRECT_GRANT_DISABLE_OTP="${GESTAUTO_DIRECT_GRANT_DISABLE_OTP:-true}"

# Dedicated client used to mint tokens for dev/tests
GESTAUTO_TOKEN_CLIENT_ID="${GESTAUTO_TOKEN_CLIENT_ID:-gestauto-dev-cli}"
GESTAUTO_TOKEN_CLIENT_PUBLIC="${GESTAUTO_TOKEN_CLIENT_PUBLIC:-false}"

kc_token() {
  curl -sS -X POST "$KEYCLOAK_BASE_URL/realms/$KEYCLOAK_ADMIN_REALM/protocol/openid-connect/token" \
    -H 'Content-Type: application/x-www-form-urlencoded' \
    --data-urlencode 'grant_type=password' \
    --data-urlencode 'client_id=admin-cli' \
    --data-urlencode "username=$KEYCLOAK_ADMIN_USER" \
    --data-urlencode "password=$KEYCLOAK_ADMIN_PASSWORD" \
    | jq -r '.access_token'
}

AUTH_HEADER=""
refresh_auth() {
  local t
  t="$(kc_token)"
  if [[ -z "$t" || "$t" == "null" ]]; then
    echo "ERROR: could not obtain admin access token. Check KEYCLOAK_ADMIN_USER/PASSWORD." >&2
    exit 1
  fi
  AUTH_HEADER="Authorization: Bearer $t"
}

kc_get() {
  local path="$1"
  curl -sS -H "$AUTH_HEADER" "$KEYCLOAK_BASE_URL/admin$path"
}

kc_post() {
  local path="$1" body="$2"
  curl -sS -o /dev/null -w '%{http_code}' \
    -H "$AUTH_HEADER" -H 'Content-Type: application/json' \
    -X POST "$KEYCLOAK_BASE_URL/admin$path" \
    -d "$body"
}

kc_put() {
  local path="$1" body="$2"
  curl -sS -o /dev/null -w '%{http_code}' \
    -H "$AUTH_HEADER" -H 'Content-Type: application/json' \
    -X PUT "$KEYCLOAK_BASE_URL/admin$path" \
    -d "$body"
}

kc_put_no_body() {
  local path="$1"
  curl -sS -o /dev/null -w '%{http_code}' \
    -H "$AUTH_HEADER" \
    -X PUT "$KEYCLOAK_BASE_URL/admin$path"
}

kc_delete() {
  local path="$1"
  curl -sS -o /dev/null -w '%{http_code}' \
    -H "$AUTH_HEADER" -X DELETE "$KEYCLOAK_BASE_URL/admin$path"
}

realm_exists() {
  local code
  code=$(curl -sS -o /dev/null -w '%{http_code}' -H "$AUTH_HEADER" "$KEYCLOAK_BASE_URL/admin/realms/$GESTAUTO_REALM")
  [[ "$code" == "200" ]]
}

ensure_realm() {
  if realm_exists; then
    echo "OK: realm '$GESTAUTO_REALM' already exists"
    return
  fi

  echo "Creating realm '$GESTAUTO_REALM'..."
  local body
  body=$(jq -n --arg realm "$GESTAUTO_REALM" '{realm:$realm, enabled:true}')
  local code
  code=$(kc_post "/realms" "$body")
  if [[ "$code" != "201" && "$code" != "204" ]]; then
    echo "ERROR: failed to create realm (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: realm created"
}

role_exists() {
  local role="$1"
  local code
  code=$(curl -sS -o /dev/null -w '%{http_code}' -H "$AUTH_HEADER" "$KEYCLOAK_BASE_URL/admin/realms/$GESTAUTO_REALM/roles/$role")
  [[ "$code" == "200" ]]
}

ensure_realm_role() {
  local role="$1"
  if role_exists "$role"; then
    echo "OK: role '$role' exists"
    return
  fi
  echo "Creating role '$role'..."
  local body
  body=$(jq -n --arg name "$role" '{name:$name}')
  local code
  code=$(kc_post "/realms/$GESTAUTO_REALM/roles" "$body")
  if [[ "$code" != "201" && "$code" != "204" ]]; then
    echo "ERROR: failed to create role '$role' (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: role created"
}

get_role_representation() {
  local role="$1"
  kc_get "/realms/$GESTAUTO_REALM/roles/$role"
}

user_id_by_username() {
  local username="$1"
  # Note: username is expected to be simple ASCII; if you need special chars, adapt to URL-encode.
  kc_get "/realms/$GESTAUTO_REALM/users?username=$username&exact=true" | jq -r '.[0].id // empty'
}

ensure_user_profile_minimum_fields() {
  local userId="$1" username="$2"

  local email firstName lastName
  email="$username@$GESTAUTO_TEST_USERS_EMAIL_DOMAIN"
  firstName="$username"
  lastName="User"

  # Fetch current user representation and patch required fields while preserving everything else.
  local current updated
  current=$(kc_get "/realms/$GESTAUTO_REALM/users/$userId")
  updated=$(echo "$current" | jq \
    --arg email "$email" \
    --arg firstName "$firstName" \
    --arg lastName "$lastName" \
    '.email = ($email) | .emailVerified = true | .firstName = ($firstName) | .lastName = ($lastName) | .enabled = true')

  local ucode
  ucode=$(kc_put "/realms/$GESTAUTO_REALM/users/$userId" "$updated")
  if [[ "$ucode" != "204" ]]; then
    echo "ERROR: failed to update user profile fields for '$username' (HTTP $ucode)" >&2
    exit 1
  fi
}

ensure_user() {
  local username="$1" password="$2"
  local id
  id=$(user_id_by_username "$username")
  if [[ -n "$id" ]]; then
    echo "OK: user '$username' exists"
  else
    echo "Creating user '$username'..."
    local body
    body=$(jq -n --arg u "$username" '{username:$u, enabled:true, emailVerified:true}')
    local code
    code=$(kc_post "/realms/$GESTAUTO_REALM/users" "$body")
    if [[ "$code" != "201" && "$code" != "204" ]]; then
      echo "ERROR: failed to create user '$username' (HTTP $code)" >&2
      exit 1
    fi
    id=$(user_id_by_username "$username")
    echo "OK: user created"
  fi

  # Ensure required user profile fields are present (email/firstName/lastName), otherwise
  # Keycloak can reject direct grant with "Account is not fully set up".
  ensure_user_profile_minimum_fields "$id" "$username"

  # Set/reset password (idempotent)
  local cred
  cred=$(jq -n --arg p "$password" '{type:"password", temporary:false, value:$p}')
  local pcode
  pcode=$(kc_put "/realms/$GESTAUTO_REALM/users/$id/reset-password" "$cred")
  if [[ "$pcode" != "204" ]]; then
    echo "ERROR: failed to set password for '$username' (HTTP $pcode)" >&2
    exit 1
  fi
}

disable_direct_grant_conditional_otp_if_requested() {
  if [[ "$GESTAUTO_DIRECT_GRANT_DISABLE_OTP" != "true" ]]; then
    return
  fi

  # Find the execution id for "Direct Grant - Conditional OTP" and disable it.
  local execId requirement
  execId=$(kc_get "/realms/$GESTAUTO_REALM/authentication/flows/direct%20grant/executions" \
    | jq -r '.[] | select(.displayName=="Direct Grant - Conditional OTP") | .id' \
    | head -n1)

  if [[ -z "$execId" ]]; then
    # Flow not present (or different Keycloak defaults) — nothing to do.
    return
  fi

  requirement=$(kc_get "/realms/$GESTAUTO_REALM/authentication/flows/direct%20grant/executions" \
    | jq -r --arg id "$execId" '.[] | select(.id==$id) | .requirement')

  if [[ "$requirement" == "DISABLED" ]]; then
    echo "OK: direct grant conditional OTP already disabled"
    return
  fi

  local body code
  body=$(jq -n --arg id "$execId" '{id:$id, requirement:"DISABLED"}')
  code=$(kc_put "/realms/$GESTAUTO_REALM/authentication/flows/direct%20grant/executions" "$body")
  if [[ "$code" != "204" ]]; then
    echo "ERROR: failed to disable direct grant conditional OTP (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: disabled direct grant conditional OTP"
}

assign_realm_roles_to_user() {
  local username="$1"; shift
  local id
  id=$(user_id_by_username "$username")
  if [[ -z "$id" ]]; then
    echo "ERROR: user '$username' not found" >&2
    exit 1
  fi

  # Build role representations array
  local reps
  reps='[]'
  for r in "$@"; do
    reps=$(jq -n --argjson reps "$reps" --argjson rep "$(get_role_representation "$r")" '$reps + [$rep]')
  done

  # Assign (POST /role-mappings/realm)
  local code
  code=$(kc_post "/realms/$GESTAUTO_REALM/users/$id/role-mappings/realm" "$reps")
  if [[ "$code" != "204" ]]; then
    echo "ERROR: failed to map roles to user '$username' (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: assigned roles to '$username': $*"
}

client_id_by_clientId() {
  local clientId="$1"
  kc_get "/realms/$GESTAUTO_REALM/clients?clientId=$clientId" | jq -r '.[0].id // empty'
}

ensure_client() {
  local clientId="$1" body="$2"
  local id
  id=$(client_id_by_clientId "$clientId")
  if [[ -n "$id" ]]; then
    echo "OK: client '$clientId' exists"
    return
  fi
  echo "Creating client '$clientId'..."
  local code
  code=$(kc_post "/realms/$GESTAUTO_REALM/clients" "$body")
  if [[ "$code" != "201" && "$code" != "204" ]]; then
    echo "ERROR: failed to create client '$clientId' (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: client created"
}

ensure_client_scope() {
  local scopeName="$1"
  local id
  id=$(kc_get "/realms/$GESTAUTO_REALM/client-scopes" | jq -r --arg n "$scopeName" '.[] | select(.name==$n) | .id' | head -n1)
  if [[ -n "$id" ]]; then
    echo "OK: client-scope '$scopeName' exists"
    return
  fi
  echo "Creating client-scope '$scopeName'..."
  local body
  body=$(jq -n --arg name "$scopeName" '{name:$name, protocol:"openid-connect"}')
  local code
  code=$(kc_post "/realms/$GESTAUTO_REALM/client-scopes" "$body")
  if [[ "$code" != "201" && "$code" != "204" ]]; then
    echo "ERROR: failed to create client-scope '$scopeName' (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: client-scope created"
}

client_scope_id() {
  local scopeName="$1"
  kc_get "/realms/$GESTAUTO_REALM/client-scopes" | jq -r --arg n "$scopeName" '.[] | select(.name==$n) | .id' | head -n1
}

ensure_protocol_mapper_user_realm_role_to_roles_claim() {
  local scopeName="$1"
  local scopeId
  scopeId=$(client_scope_id "$scopeName")

  local existing
  existing=$(kc_get "/realms/$GESTAUTO_REALM/client-scopes/$scopeId/protocol-mappers/models" | jq -r '.[] | select(.name=="roles-claim") | .id' | head -n1)
  if [[ -n "$existing" ]]; then
    echo "OK: protocol mapper 'roles-claim' already exists in scope '$scopeName'"
    return
  fi

  echo "Creating protocol mapper 'roles-claim' in scope '$scopeName'..."
  local body
  body=$(jq -n '(
    {
      name: "roles-claim",
      protocol: "openid-connect",
      protocolMapper: "oidc-usermodel-realm-role-mapper",
      consentRequired: false,
      config: {
        "multivalued": "true",
        "userinfo.token.claim": "false",
        "id.token.claim": "false",
        "access.token.claim": "true",
        "claim.name": "roles",
        "jsonType.label": "String"
      }
    }
  )')

  local code
  code=$(kc_post "/realms/$GESTAUTO_REALM/client-scopes/$scopeId/protocol-mappers/models" "$body")
  if [[ "$code" != "201" && "$code" != "204" ]]; then
    echo "ERROR: failed to create roles mapper (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: roles mapper created"
}

ensure_protocol_mapper_audience_included_client() {
  local scopeName="$1" includedAudience="$2" mapperName="$3"
  local scopeId
  scopeId=$(client_scope_id "$scopeName")

  local existing
  existing=$(kc_get "/realms/$GESTAUTO_REALM/client-scopes/$scopeId/protocol-mappers/models" | jq -r --arg n "$mapperName" '.[] | select(.name==$n) | .id' | head -n1)
  if [[ -n "$existing" ]]; then
    echo "OK: protocol mapper '$mapperName' already exists in scope '$scopeName'"
    return
  fi

  echo "Creating audience mapper '$mapperName' (include '$includedAudience') in scope '$scopeName'..."
  local body
  body=$(jq -n --arg ia "$includedAudience" --arg n "$mapperName" '(
    {
      name: $n,
      protocol: "openid-connect",
      protocolMapper: "oidc-audience-mapper",
      consentRequired: false,
      config: {
        "included.client.audience": $ia,
        "id.token.claim": "false",
        "access.token.claim": "true"
      }
    }
  )')

  local code
  code=$(kc_post "/realms/$GESTAUTO_REALM/client-scopes/$scopeId/protocol-mappers/models" "$body")
  if [[ "$code" != "201" && "$code" != "204" ]]; then
    echo "ERROR: failed to create audience mapper (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: audience mapper created"
}

attach_default_scope_to_client() {
  local clientId="$1" scopeName="$2"
  local clientInternalId
  clientInternalId=$(client_id_by_clientId "$clientId")
  if [[ -z "$clientInternalId" ]]; then
    echo "ERROR: client '$clientId' not found" >&2
    exit 1
  fi

  local scopeId
  scopeId=$(client_scope_id "$scopeName")
  if [[ -z "$scopeId" ]]; then
    echo "ERROR: client-scope '$scopeName' not found" >&2
    exit 1
  fi

  # Check already attached
  local attached
  attached=$(kc_get "/realms/$GESTAUTO_REALM/clients/$clientInternalId/default-client-scopes" | jq -r --arg sid "$scopeId" '.[] | select(.id==$sid) | .id' | head -n1)
  if [[ -n "$attached" ]]; then
    echo "OK: scope '$scopeName' already attached as default to '$clientId'"
    return
  fi

  # Attach scope as default (PUT with no body is accepted across Keycloak versions)
  local code
  code=$(kc_put_no_body "/realms/$GESTAUTO_REALM/clients/$clientInternalId/default-client-scopes/$scopeId")
  if [[ "$code" != "204" ]]; then
    echo "ERROR: failed to attach scope '$scopeName' to '$clientId' (HTTP $code)" >&2
    exit 1
  fi
  echo "OK: attached default scope '$scopeName' to '$clientId'"
}

get_client_secret() {
  local clientId="$1"
  local clientInternalId
  clientInternalId=$(client_id_by_clientId "$clientId")
  if [[ -z "$clientInternalId" ]]; then
    echo ""; return
  fi
  kc_get "/realms/$GESTAUTO_REALM/clients/$clientInternalId/client-secret" | jq -r '.value // empty'
}

main() {
  refresh_auth

  ensure_realm

  # Keep local token scripts functional.
  disable_direct_grant_conditional_otp_if_requested

  echo "\n== Realm roles =="
  ensure_realm_role ADMIN
  ensure_realm_role MANAGER
  ensure_realm_role VIEWER
  ensure_realm_role SALES_PERSON
  ensure_realm_role SALES_MANAGER
  ensure_realm_role VEHICLE_EVALUATOR
  ensure_realm_role EVALUATION_MANAGER

  echo "\n== Test users =="
  ensure_user admin admin
  ensure_user sales_manager 123456
  ensure_user seller 123456
  ensure_user eval_manager 123456
  ensure_user evaluator 123456

  # Assign roles (realm-level)
  assign_realm_roles_to_user admin ADMIN MANAGER
  assign_realm_roles_to_user sales_manager MANAGER SALES_MANAGER SALES_PERSON
  assign_realm_roles_to_user seller SALES_PERSON
  assign_realm_roles_to_user eval_manager MANAGER EVALUATION_MANAGER VEHICLE_EVALUATOR
  assign_realm_roles_to_user evaluator VEHICLE_EVALUATOR

  echo "\n== API clients (resource servers) =="
  # NOTE: Recommended: keep APIs as bearer-only resource servers.
  # If you need to mint tokens, use the dedicated dev client below.
  local commercialClient
  commercialClient=$(jq -n '{clientId:"gestauto-commercial-api", protocol:"openid-connect", bearerOnly:true, enabled:true, publicClient:false, standardFlowEnabled:false, directAccessGrantsEnabled:false, serviceAccountsEnabled:false}')
  ensure_client gestauto-commercial-api "$commercialClient"

  local vehicleClient
  vehicleClient=$(jq -n '{clientId:"vehicle-evaluation-api", protocol:"openid-connect", bearerOnly:true, enabled:true, publicClient:false, standardFlowEnabled:false, directAccessGrantsEnabled:false, serviceAccountsEnabled:false}')
  ensure_client vehicle-evaluation-api "$vehicleClient"

  echo "\n== Frontend client (SPA) =="
  # Public SPA client for browser-based login using Authorization Code + PKCE.
  # Note: The realm can be switched per environment via GESTAUTO_REALM.
  local frontendClient
  frontendClient=$(jq -n '{
    clientId:"gestauto-frontend",
    protocol:"openid-connect",
    enabled:true,
    publicClient:true,
    bearerOnly:false,
    standardFlowEnabled:true,
    implicitFlowEnabled:false,
    directAccessGrantsEnabled:false,
    serviceAccountsEnabled:false,
    redirectUris:["http://gestauto.tasso.local/*"],
    webOrigins:["http://gestauto.tasso.local"],
    attributes:{
      "pkce.code.challenge.method":"S256"
    }
  }')
  ensure_client gestauto-frontend "$frontendClient"

  echo "\n== Client scopes and protocol mappers =="
  ensure_client_scope gestauto-roles
  ensure_protocol_mapper_user_realm_role_to_roles_claim gestauto-roles

  ensure_client_scope gestauto-audiences
  ensure_protocol_mapper_audience_included_client gestauto-audiences gestauto-commercial-api audience-commercial
  ensure_protocol_mapper_audience_included_client gestauto-audiences vehicle-evaluation-api audience-vehicle

  # Attach scopes to the SPA client so tokens include 'roles' (and optionally API audiences).
  attach_default_scope_to_client gestauto-frontend gestauto-roles
  attach_default_scope_to_client gestauto-frontend gestauto-audiences

  echo "\n== Token minting client (dev/test) =="
  # This client is used to generate tokens that include:
  # - roles claim ("roles")
  # - aud including the API clients
  # It can be public or confidential.
  if [[ "$GESTAUTO_TOKEN_CLIENT_PUBLIC" == "true" ]]; then
    local tokenClient
    tokenClient=$(jq -n --arg cid "$GESTAUTO_TOKEN_CLIENT_ID" '{clientId:$cid, protocol:"openid-connect", enabled:true, publicClient:true, standardFlowEnabled:false, directAccessGrantsEnabled:true, serviceAccountsEnabled:false, bearerOnly:false}')
    ensure_client "$GESTAUTO_TOKEN_CLIENT_ID" "$tokenClient"
  else
    local tokenClient
    tokenCls://gestauto.tasso.local/*",
      "http://gestauto.tasso.local/*",
      "http://localhost:5173/*",
      "http://localhost:4173/*"
    ],
    webOrigins:[
      "https://gestauto.tasso.local",_scope_to_client "$GESTAUTO_TOKEN_CLIENT_ID" gestauto-roles
  attach_default_scope_to_client "$GESTAUTO_TOKEN_CLIENT_ID" gestauto-audiences

  if [[ "$GESTAUTO_TOKEN_CLIENT_PUBLIC" != "true" ]]; then
    echo "\n== Generated secret for $GESTAUTO_TOKEN_CLIENT_ID =="
    echo "CLIENT_SECRET=$(get_client_secret "$GESTAUTO_TOKEN_CLIENT_ID")"
  fi

  echo "\nDONE."
  echo "Next: generate a token using $GESTAUTO_TOKEN_CLIENT_ID and check that JWT contains 'roles' claim and 'aud' includes the API client IDs."
}

main "$@"
