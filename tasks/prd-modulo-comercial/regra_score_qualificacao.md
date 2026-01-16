Olá. Como executivo comercial, minha prioridade não é apenas "vender o carro", mas **maximizar a rentabilidade da operação (margem de lucro)** e o **giro de estoque**.

Muitos vendedores cometem o erro de achar que o comprador "à vista" é o melhor lead. Nem sempre. Para a concessionária, o financiamento gera retorno bancário (retorno financeiro sobre o contrato) e o usado na troca gera uma nova oportunidade de lucro na revenda.

Abaixo, apresento uma **Matriz de Classificação de Leads (Lead Scoring)** baseada em *Potencial de Lucratividade* e *Probabilidade de Fechamento*.

---

### 1. A Lógica Estratégica (O "Pulo do Gato")

Antes de classificar, entenda como o "Board" vê cada cenário financeiramente:

* **Cenário 1 (À Vista):** Ótimo para fluxo de caixa imediato, mas margem de lucro limitada (geralmente o cliente pede muito desconto).
* **Cenário 2 (À Vista + Usado):** Bom caixa + Lucro futuro na revenda do usado.
* **Cenário 3 (Financiado):** Lucro da venda + Comissão do banco (retorno financeiro).
* **Cenário 4 (Financiado + Usado):** O "Santo Graal". Lucro da venda + Comissão do banco + Lucro na revenda do usado.

---

### 2. Classificação dos Leads (SLA de Atendimento)

Vamos dividir os leads em 4 categorias para definir a prioridade da equipe de vendas.

#### 💎 Lead Diamante (Prioridade Máxima - Atender em até 10 min)
Estes são os clientes que trazem a **maior margem combinada** para a loja.
* **Perfil:** Cenário 4 (Financiado + Usado).
* **Critério Adicional:** O usado é de alta liquidez (fácil de vender) e o cliente busca um veículo 0km de alto valor agregado (SUV ou Premium).
* **Ação:** Gerente deve acompanhar a negociação de perto.

#### 🥇 Lead Ouro (Alta Prioridade - Atender em até 30 min)
Clientes com alta probabilidade de fechamento ou alta rentabilidade única.
* **Perfil:** Cenário 2 (À vista + Usado) **OU** Cenário 3 (Financiado sem troca).
* **Critério Adicional:** Cliente com "Score" de crédito pré-aprovado ou dinheiro disponível imediato.
* **Ação:** Vendedor sênior focado em fechar rápido ou vender acessórios agregados (F&I).

#### 🥈 Lead Prata (Média Prioridade - Atender em até 2 horas)
O cliente "transacional". A venda é rápida, mas a margem é "espremida".
* **Perfil:** Cenário 1 (À vista puro).
* **Critério Adicional:** Cliente pedindo muito desconto ou cotando em várias concessionárias ao mesmo tempo.
* **Ação:** Vendedor focado em defender o preço e tentar converter para um financiamento parcial (taxa zero) para ganhar na fidelização.

#### 🥉 Lead Bronze (Baixa Prioridade - Nutrição Automática)
Leads frios ou especulativos.
* **Perfil:** Curiosos ou clientes com restrição de crédito grave.
* **Ação:** Colocar em fluxo de automação de marketing (e-mail/WhatsApp) até demonstrar sinal de compra real.

---

### 3. Critérios Extras de Qualificação (Scorecard)

Para refinar sua classificação e não depender apenas da forma de pagamento, adicionei estes critérios cruciais para o setor automotivo:

| Critério | Peso na Decisão | O que valorizamos |
| :--- | :---: | :--- |
| **Tempo de Compra** | Alto | **"Imediato"** ou **"Até 7 dias"**. Se for "daqui a 3 meses", a prioridade cai. |
| **Modelo de Interesse** | Médio | Carros em **estoque parado** (precisamos desovar) valem mais pontos que lançamentos (que se vendem sozinhos). |
| **Origem do Lead** | Alto | **Showroom/Telefone** (Lead quente) > **Indicação** > **Site Próprio** > **Portal de Classificados** (Lead frio/leilão de preço). |
| **Estado do Usado** | Médio | Carro de troca com **baixa quilometragem e revisões na marca** sobe o lead de nível imediatamente. |

---

### 4. Resumo da Regra de Negócio (Para implementação no CRM)

Se você fosse configurar isso no seu sistema CRM, a regra seria:

1.  **Se** (Financiado + Usado) **E** (Compra em < 15 dias) = **Lead Diamante** (Ligar Agora).
2.  **Se** (À Vista + Usado) **OU** (Financiado) **E** (Compra em < 15 dias) = **Lead Ouro**.
3.  **Se** (À Vista puro) = **Lead Prata** (Gerenciar expectativa de desconto).
4.  **Se** (Compra > 30 dias) = **Lead Bronze** (Nutrição).
