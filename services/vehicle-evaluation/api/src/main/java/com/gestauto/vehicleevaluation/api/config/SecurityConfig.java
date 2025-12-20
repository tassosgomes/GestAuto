package com.gestauto.vehicleevaluation.api.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.method.configuration.EnableMethodSecurity;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.annotation.web.configuration.EnableWebSecurity;
import org.springframework.security.config.annotation.web.configurers.AbstractHttpConfigurer;
import org.springframework.security.config.http.SessionCreationPolicy;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.oauth2.server.resource.authentication.JwtAuthenticationConverter;
import org.springframework.security.oauth2.server.resource.authentication.JwtGrantedAuthoritiesConverter;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.security.web.AuthenticationEntryPoint;
import org.springframework.security.web.SecurityFilterChain;

/**
 * Configuração de segurança para a API de Avaliação de Veículos.
 *
 * Esta configuração define as regras de segurança, autenticação JWT via OAuth2 Resource Server
 * e autorização baseada em roles para os endpoints da API.
 * 
 * Roles seguem o padrão SCREAMING_SNAKE_CASE conforme definido em:
 * rules/ROLES_NAMING_CONVENTION.md
 */
@Configuration
@EnableWebSecurity
@EnableMethodSecurity(prePostEnabled = true)
public class SecurityConfig {

    /**
     * Configuração principal de segurança HTTP.
     *
     * @param http configuração HTTP do Spring Security
     * @return SecurityFilterChain configurado
     * @throws Exception em caso de erro na configuração
     */
    @Bean
    public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
        http
            // Desabilita CSRF (comum para APIs REST)
            .csrf(AbstractHttpConfigurer::disable)

            // Configura política de sessão stateless (para APIs REST)
            .sessionManagement(session -> session
                .sessionCreationPolicy(SessionCreationPolicy.STATELESS)
            )

            // Configura regras de autorização
            // Roles padronizadas conforme ROLES_NAMING_CONVENTION.md
            .authorizeHttpRequests(authz -> authz
                // Endpoints públicos
                .requestMatchers("/actuator/health").permitAll()
                .requestMatchers("/actuator/info").permitAll()
                .requestMatchers("/v3/api-docs/**").permitAll()
                .requestMatchers("/api-docs/**").permitAll()
                .requestMatchers("/api/api-docs/**").permitAll()
                .requestMatchers("/api/v3/api-docs/**").permitAll()
                .requestMatchers("/swagger-ui/**").permitAll()
                .requestMatchers("/swagger-ui.html").permitAll()
                .requestMatchers("/swagger-ui/index.html").permitAll()

                // Validação pública de laudos (QR code)
                .requestMatchers("/api/v1/evaluations/public/**").permitAll()

                // Endpoints que requerem autenticação
                .requestMatchers("/api/v1/evaluations/**")
                    .hasAnyRole("VEHICLE_EVALUATOR", "EVALUATION_MANAGER", "MANAGER", "ADMIN")

                // Qualquer outra requisição requer autenticação
                .anyRequest().authenticated()
            )

            // Configura OAuth2 Resource Server com JWT
            .oauth2ResourceServer(oauth2 -> oauth2
                .authenticationEntryPoint(authenticationEntryPoint())
                .jwt(jwt -> jwt
                    .jwtAuthenticationConverter(jwtAuthenticationConverter())
                )
            )

            // Configura CORS (se necessário)
            .cors(cors -> cors.configure(http));

        return http.build();
    }

    /**
     * Converter de JWT para Authentication do Spring Security.
     * 
     * Configura a leitura das roles a partir da claim "roles" do token JWT
     * e adiciona o prefixo "ROLE_" automaticamente para compatibilidade
     * com hasRole() e hasAnyRole() do Spring Security.
     *
     * @return JwtAuthenticationConverter configurado
     */
    @Bean
    public JwtAuthenticationConverter jwtAuthenticationConverter() {
        JwtGrantedAuthoritiesConverter grantedAuthoritiesConverter = new JwtGrantedAuthoritiesConverter();
        // Claim padronizada conforme ROLES_NAMING_CONVENTION.md
        grantedAuthoritiesConverter.setAuthoritiesClaimName("roles");
        // Spring Security espera prefixo ROLE_ internamente
        grantedAuthoritiesConverter.setAuthorityPrefix("ROLE_");

        JwtAuthenticationConverter jwtAuthenticationConverter = new JwtAuthenticationConverter();
        jwtAuthenticationConverter.setJwtGrantedAuthoritiesConverter(grantedAuthoritiesConverter);
        return jwtAuthenticationConverter;
    }

    /**
     * Bean para encoder de senhas.
     *
     * @return PasswordEncoder configurado com BCrypt
     */
    @Bean
    public PasswordEncoder passwordEncoder() {
        return new BCryptPasswordEncoder();
    }

    /**
     * AuthenticationEntryPoint personalizado para OAuth2 Resource Server.
     *
     * Permite acesso anônimo aos endpoints públicos do Swagger/OpenAPI,
     * mas requer autenticação para outros endpoints.
     *
     * @return AuthenticationEntryPoint configurado
     */
    @Bean
    public AuthenticationEntryPoint authenticationEntryPoint() {
        return (request, response, authException) -> {
            String requestURI = request.getRequestURI();
            // Permitir anonymous para endpoints do Swagger/OpenAPI
            if (requestURI.startsWith("/api/v3/api-docs") ||
                requestURI.startsWith("/api/swagger-ui") ||
                requestURI.startsWith("/api/actuator/health") ||
                requestURI.startsWith("/api/actuator/info")) {
                // Não fazer nada - permitir anonymous
                return;
            }
            // Para outros endpoints, retornar 401
            response.sendError(HttpServletResponse.SC_UNAUTHORIZED, "Authentication required");
        };
    }
}