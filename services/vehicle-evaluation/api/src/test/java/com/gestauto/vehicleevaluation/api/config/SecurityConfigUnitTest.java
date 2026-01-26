package com.gestauto.vehicleevaluation.api.config;

import static org.assertj.core.api.Assertions.assertThat;

import java.util.List;

import org.junit.jupiter.api.Test;
import org.springframework.mock.web.MockHttpServletRequest;
import org.springframework.mock.web.MockHttpServletResponse;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.oauth2.jwt.Jwt;

class SecurityConfigUnitTest {

    @Test
    void passwordEncoderEncodesAndMatches() {
        SecurityConfig config = new SecurityConfig();

        var encoder = config.passwordEncoder();
        String hash = encoder.encode("secret");

        assertThat(encoder.matches("secret", hash)).isTrue();
        assertThat(encoder.matches("wrong", hash)).isFalse();
    }

    @Test
    void jwtAuthenticationConverterReadsRolesClaim() {
        SecurityConfig config = new SecurityConfig();

        var converter = config.jwtAuthenticationConverter();

        Jwt jwt = Jwt.withTokenValue("token")
            .header("alg", "none")
            .claim("roles", List.of("ADMIN"))
            .build();

        Authentication authentication = converter.convert(jwt);
        assertThat(authentication).isNotNull();

        List<String> authorities = authentication.getAuthorities().stream()
            .map(GrantedAuthority::getAuthority)
            .toList();

        assertThat(authorities).contains("ROLE_ADMIN");
    }

    @Test
    void authenticationEntryPointReturns401ForNonSwaggerEndpoints() throws Exception {
        SecurityConfig config = new SecurityConfig();

        var entryPoint = config.authenticationEntryPoint();

        MockHttpServletRequest request = new MockHttpServletRequest("GET", "/api/v1/evaluations/secure");
        MockHttpServletResponse response = new MockHttpServletResponse();

        entryPoint.commence(request, response, null);

        assertThat(response.getStatus()).isEqualTo(401);
    }

    @Test
    void authenticationEntryPointDoesNothingForApiDocsEndpoints() throws Exception {
        SecurityConfig config = new SecurityConfig();

        var entryPoint = config.authenticationEntryPoint();

        MockHttpServletRequest request = new MockHttpServletRequest("GET", "/vehicle-evaluation/api/v3/api-docs");
        request.setContextPath("/vehicle-evaluation/api");
        MockHttpServletResponse response = new MockHttpServletResponse();
        response.setStatus(200);

        entryPoint.commence(request, response, null);

        assertThat(response.getStatus()).isEqualTo(200);
    }
}
