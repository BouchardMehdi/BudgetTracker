CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(80) NOT NULL UNIQUE,
    email VARCHAR(180) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    type VARCHAR(20) NOT NULL,
    user_id INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ck_categories_type CHECK (type IN ('income', 'expense')),
    CONSTRAINT fk_categories_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_categories_user_name_type UNIQUE (user_id, name, type)
);

CREATE TABLE IF NOT EXISTS transactions (
    id SERIAL PRIMARY KEY,
    title VARCHAR(160) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    type VARCHAR(20) NOT NULL,
    transaction_date DATE NOT NULL,
    description VARCHAR(1000),
    category_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    is_recurring BOOLEAN NOT NULL DEFAULT FALSE,
    recurrence_start_date DATE,
    recurrence_end_date DATE,
    recurring_parent_id INTEGER,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ck_transactions_amount CHECK (amount > 0),
    CONSTRAINT ck_transactions_type CHECK (type IN ('income', 'expense')),
    CONSTRAINT fk_transactions_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_transactions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_transactions_recurring_parent FOREIGN KEY (recurring_parent_id) REFERENCES transactions(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_categories_user_id ON categories(user_id);
CREATE INDEX IF NOT EXISTS ix_transactions_user_id ON transactions(user_id);
CREATE INDEX IF NOT EXISTS ix_transactions_category_id ON transactions(category_id);
CREATE INDEX IF NOT EXISTS ix_transactions_transaction_date ON transactions(transaction_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_transactions_recurring_parent_date ON transactions(recurring_parent_id, transaction_date);

CREATE TABLE IF NOT EXISTS budgets (
    id SERIAL PRIMARY KEY,
    amount NUMERIC(18, 2) NOT NULL,
    category_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ck_budgets_amount CHECK (amount > 0),
    CONSTRAINT fk_budgets_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE CASCADE,
    CONSTRAINT fk_budgets_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_budgets_user_category UNIQUE (user_id, category_id)
);
