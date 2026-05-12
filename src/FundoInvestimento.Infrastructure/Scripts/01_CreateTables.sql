-- Criação da tabela de Clientes
CREATE TABLE IF NOT EXISTS cliente (
    id UUID PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    cpf VARCHAR(11) UNIQUE NOT NULL,
    saldo_disponivel NUMERIC(18, 2) NOT NULL DEFAULT 0.00
);

-- Criação da tabela de Fundos
CREATE TABLE IF NOT EXISTS fundo (
    id UUID PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    horario_corte TIME NOT NULL,
    valor_cota NUMERIC(18, 6) NOT NULL,
    valor_minimo_aporte NUMERIC(18, 2) NOT NULL,
    valor_minimo_permanencia NUMERIC(18, 2) NOT NULL,
    status_captacao VARCHAR(20) NOT NULL -- 'ABERTO' ou 'FECHADO'
);

-- Criação da tabela de Posição do Cliente (Relacionamento N:N)
CREATE TABLE IF NOT EXISTS posicao_cliente (
    id_cliente UUID NOT NULL REFERENCES cliente(id),
    id_fundo UUID NOT NULL REFERENCES fundo(id),
    quantidade_cotas INTEGER NOT NULL CHECK (quantidade_cotas >= 0),
    PRIMARY KEY (id_cliente, id_fundo)
);

-- Criação da tabela de Ordens
CREATE TABLE IF NOT EXISTS ordem (
    id UUID PRIMARY KEY,
    id_cliente UUID NOT NULL REFERENCES cliente(id),
    id_fundo UUID NOT NULL REFERENCES fundo(id),
    tipo_operacao VARCHAR(20) NOT NULL, -- 'APORTE' ou 'RESGATE'
    quantidade_cotas INTEGER NOT NULL CHECK (quantidade_cotas > 0),
    data_agendamento DATE NULL, -- Nulo para imediatas
    status VARCHAR(20) NOT NULL, -- 'PENDENTE', 'CONCLUIDO', 'REJEITADO'
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Criação de índice de ordem para acelerar consultas por cliente
CREATE INDEX IF NOT EXISTS idx_ordem_id_cliente ON ordem(id_cliente);

-- Criação de índice de ordem para acelerar consultas ordenadas por data de criação
CREATE INDEX IF NOT EXISTS idx_ordem_criado_em ON ordem(criado_em DESC);


-- Popular tabelas com dados iniciais para testes

-- Cliente 1: Com saldo para testes de aporte
INSERT INTO cliente (id, nome, cpf, saldo_disponivel) 
VALUES ('11111111-1111-1111-1111-111111111111', 'Raul Souza', '12345678901', 50000.00)
ON CONFLICT (id) DO NOTHING;

-- Cliente 2: Sem saldo para testes de resgate/rejeição
INSERT INTO cliente (id, nome, cpf, saldo_disponivel) 
VALUES ('22222222-2222-2222-2222-222222222222', 'Joao Silva', '10987654321', 0.00)
ON CONFLICT (id) DO NOTHING;

-- Fundo 1: Fundo Multimercado (Aberto, corte 14:00, cota R$ 10, min aporte R$ 100)
INSERT INTO fundo (id, nome, horario_corte, valor_cota, valor_minimo_aporte, valor_minimo_permanencia, status_captacao)
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Fundo Multimercado Alpha', '14:00:00', 10.000000, 100.00, 50.00, 'ABERTO')
ON CONFLICT (id) DO NOTHING;

-- Fundo 2: Fundo de Ações (Aberto, corte 16:00, cota R$ 12.50, min aporte R$ 500)
INSERT INTO fundo (id, nome, horario_corte, valor_cota, valor_minimo_aporte, valor_minimo_permanencia, status_captacao)
VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Fundo de Ações Beta', '16:00:00', 12.500000, 500.00, 250.00, 'ABERTO')
ON CONFLICT (id) DO NOTHING;

-- Fundo 3: Fundo Cambial (Fechado - Capacity atingido)
INSERT INTO fundo (id, nome, horario_corte, valor_cota, valor_minimo_aporte, valor_minimo_permanencia, status_captacao)
VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Fundo Cambial Gama', '15:00:00', 5.000000, 50.00, 0.00, 'FECHADO')
ON CONFLICT (id) DO NOTHING;

INSERT INTO ordem (id, id_cliente, id_fundo, tipo_operacao, quantidade_cotas, data_agendamento, status, criado_em)
VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 
    '11111111-1111-1111-1111-111111111111',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'APORTE', 
    500, 
    NULL, 
    'CONCLUIDO', 
    NOW() - INTERVAL '1 day'
) ON CONFLICT (id) DO NOTHING;

INSERT INTO posicao_cliente (id_cliente, id_fundo, quantidade_cotas)
VALUES ('11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 500)
ON CONFLICT (id_cliente, id_fundo) DO NOTHING;