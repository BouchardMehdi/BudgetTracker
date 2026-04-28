INSERT INTO users (id, username, email, password_hash, created_at)
VALUES (1, 'demo', 'demo@budgettracker.local', 'not_used_in_mvp', NOW())
ON CONFLICT (id) DO NOTHING;

INSERT INTO categories (id, name, type, user_id, created_at)
VALUES
    (1, 'Salaire', 'income', 1, NOW()),
    (2, 'Freelance', 'income', 1, NOW()),
    (3, 'Courses', 'expense', 1, NOW()),
    (4, 'Logement', 'expense', 1, NOW()),
    (5, 'Transport', 'expense', 1, NOW()),
    (6, 'Loisirs', 'expense', 1, NOW())
ON CONFLICT (id) DO NOTHING;

INSERT INTO transactions (title, amount, type, transaction_date, description, category_id, user_id, created_at, updated_at)
VALUES
    ('Salaire avril', 2800.00, 'income', '2026-04-01', 'Salaire mensuel', 1, 1, NOW(), NOW()),
    ('Mission landing page', 450.00, 'income', '2026-04-08', 'Projet freelance', 2, 1, NOW(), NOW()),
    ('Supermarche', 86.45, 'expense', '2026-04-05', 'Courses semaine', 3, 1, NOW(), NOW()),
    ('Loyer', 920.00, 'expense', '2026-04-03', 'Appartement', 4, 1, NOW(), NOW()),
    ('Metro', 35.00, 'expense', '2026-04-10', 'Recharge transport', 5, 1, NOW(), NOW()),
    ('Cinema', 24.00, 'expense', '2026-04-12', 'Sortie weekend', 6, 1, NOW(), NOW());

SELECT setval('users_id_seq', (SELECT MAX(id) FROM users));
SELECT setval('categories_id_seq', (SELECT MAX(id) FROM categories));
