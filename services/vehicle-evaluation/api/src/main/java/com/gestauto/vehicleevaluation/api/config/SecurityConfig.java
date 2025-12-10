package com.gestauto.vehicleevaluation.api.config;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpMethod;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.annotation.web.configuration.EnableWebSecurity;
import org.springframework.security.config.annotation.web.configurers.AbstractHttpConfigurer;
import org.springframework.security.config.http.SessionCreationPolicy;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;

/**
 * Configuração de segurança para a API de Avaliação de Veículos.
 *
 * Esta configuração define as regras de segurança, autenticação JWT
 * e autorização baseada em roles para os endpoints da API.
 */
@Configuration
@EnableWebSecurity
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
            .authorizeHttpRequests(authz -> authz
                // Endpoints públicos
                .requestMatchers("/actuator/health").permitAll()
                .requestMatchers("/actuator/info").permitAll()
                .requestMatchers("/v3/api-docs/**").permitAll()
                .requestMatchers("/swagger-ui/**").permitAll()
                .requestMatchers("/swagger-ui.html").permitAll()

                // Endpoints que requerem autenticação
                .requestMatchers("/api/v1/evaluations/**")
                    .hasAnyRole("EVALUATOR", "MANAGER", "ADMIN")

                // Qualquer outra requisição requer autenticação
                .anyRequest().authenticated()
            )

            // Configura CORS (se necessário)
            .cors(cors -> cors.configure(http));

        return http.build();
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
}