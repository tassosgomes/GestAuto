# Diagrama Mermaid — Módulo de Estoque (Veículos)

Este diagrama representa os **status vigentes** do veículo no estoque e as **transições principais** descritas no PRD (check-in, reserva, test-drive, preparação, venda e baixa).

```mermaid
stateDiagram-v2
    direction LR

  [*] --> em_transito: check-in (origem montadora / transferência)
  [*] --> em_estoque: check-in (origem compra seminovo / frota interna)

    %% ========
    %% Status base (RF3)
    %% ========
    state "em_transito" as em_transito
    state "em_estoque" as em_estoque
    state "reservado" as reservado
    state "em_test_drive" as em_test_drive
    state "em_preparacao" as em_preparacao
    state "vendido" as vendido
    state "baixado" as baixado

    %% ========
    %% Evolução operacional
    %% ========
    em_transito --> em_estoque: chegada / conferência

    em_estoque --> em_preparacao: iniciar preparação (oficina)
    em_preparacao --> em_estoque: concluir preparação (pronto para venda)

    %% ========
    %% Reserva (RF6)
    %% ========
    em_estoque --> reservado: reservar (vendedor)
    reservado --> em_estoque: cancelar reserva (vendedor dono / gerente)
    reservado --> em_estoque: expirar reserva (48h) tipo=padrao

    note right of reservado
      Tipos de reserva (RF6.7):
      - padrao (expira 48h)
      - entrada_paga (não expira)
      - aguardando_banco (prazo informado)

      Cancelamento:
      - próprio vendedor pode cancelar a própria reserva
      - gerente comercial pode cancelar reserva de outro vendedor
    end note

    %% ========
    %% Test-drive (RF7)
    %% ========
    em_estoque --> em_test_drive: iniciar test-drive
    reservado --> em_test_drive: iniciar test-drive (mantendo vínculo comercial)

    em_test_drive --> em_estoque: encerrar test-drive (devolvido)
    em_test_drive --> reservado: encerrar test-drive (vira negociação)

    %% ========
    %% Saída / venda / baixa (RF5)
    %% ========
    reservado --> vendido: checkout (motivo venda)
    em_estoque --> vendido: checkout (motivo venda)

    em_estoque --> baixado: checkout (motivo baixa)
    reservado --> baixado: checkout (motivo baixa)
    em_preparacao --> baixado: checkout (motivo baixa)

    vendido --> [*]
    baixado --> [*]

    note right of em_estoque
      Regras gerais:
      - Status vigente é único (RF3.1)
      - Mudanças devem ser auditáveis (RF3.2, RF9)
      - Ações incompatíveis com status devem ser bloqueadas (RF3.3)

      Motivos mínimos de checkout (RF5):
      - venda
      - test_drive
      - transferencia
      - baixa_sinistro_perda_total
    end note
```
