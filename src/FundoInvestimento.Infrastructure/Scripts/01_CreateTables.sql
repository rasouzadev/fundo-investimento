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