DO $$
DECLARE
    demo_user_id INTEGER;
    salaire_id INTEGER;
    freelance_id INTEGER;
    remboursement_id INTEGER;
    courses_id INTEGER;
    logement_id INTEGER;
    transport_id INTEGER;
    loisirs_id INTEGER;
    sante_id INTEGER;
    abonnements_id INTEGER;
    restaurants_id INTEGER;
    shopping_id INTEGER;
    vacances_id INTEGER;
BEGIN
    INSERT INTO users (username, email, password_hash, created_at)
    VALUES (
        'demo',
        'demo@budgettracker.local',
        'pbkdf2_sha256$100000$pR7h+kkgz4HywbXPP5z+FA==$LRFszK/W7tX+xtjY20C3BMS0cVEkSVquqeM8M322c3g=',
        NOW()
    )
    ON CONFLICT (email) DO UPDATE
    SET
        username = EXCLUDED.username,
        password_hash = EXCLUDED.password_hash;

    SELECT id INTO demo_user_id
    FROM users
    WHERE email = 'demo@budgettracker.local';

    DELETE FROM transactions WHERE user_id = demo_user_id;
    DELETE FROM categories WHERE user_id = demo_user_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Salaire', 'income', demo_user_id, NOW())
    RETURNING id INTO salaire_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Freelance', 'income', demo_user_id, NOW())
    RETURNING id INTO freelance_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Remboursements', 'income', demo_user_id, NOW())
    RETURNING id INTO remboursement_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Courses', 'expense', demo_user_id, NOW())
    RETURNING id INTO courses_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Logement', 'expense', demo_user_id, NOW())
    RETURNING id INTO logement_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Transport', 'expense', demo_user_id, NOW())
    RETURNING id INTO transport_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Loisirs', 'expense', demo_user_id, NOW())
    RETURNING id INTO loisirs_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Sante', 'expense', demo_user_id, NOW())
    RETURNING id INTO sante_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Abonnements', 'expense', demo_user_id, NOW())
    RETURNING id INTO abonnements_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Restaurants', 'expense', demo_user_id, NOW())
    RETURNING id INTO restaurants_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Shopping', 'expense', demo_user_id, NOW())
    RETURNING id INTO shopping_id;

    INSERT INTO categories (name, type, user_id, created_at)
    VALUES ('Vacances', 'expense', demo_user_id, NOW())
    RETURNING id INTO vacances_id;

    INSERT INTO transactions (title, amount, type, transaction_date, description, category_id, user_id, created_at, updated_at)
    VALUES
        ('Salaire avril', 2850.00, 'income', '2026-04-01', 'Salaire mensuel', salaire_id, demo_user_id, NOW(), NOW()),
        ('Mission React', 620.00, 'income', '2026-04-09', 'Projet freelance court', freelance_id, demo_user_id, NOW(), NOW()),
        ('Remboursement mutuelle', 42.50, 'income', '2026-04-16', 'Consultation rembourseee', remboursement_id, demo_user_id, NOW(), NOW()),
        ('Loyer avril', 920.00, 'expense', '2026-04-03', 'Appartement', logement_id, demo_user_id, NOW(), NOW()),
        ('Supermarche semaine 1', 84.30, 'expense', '2026-04-05', 'Courses alimentaires', courses_id, demo_user_id, NOW(), NOW()),
        ('Pass Navigo', 86.40, 'expense', '2026-04-06', 'Transport mensuel', transport_id, demo_user_id, NOW(), NOW()),
        ('Netflix', 13.49, 'expense', '2026-04-07', 'Abonnement video', abonnements_id, demo_user_id, NOW(), NOW()),
        ('Restaurant italien', 48.20, 'expense', '2026-04-11', 'Diner samedi', restaurants_id, demo_user_id, NOW(), NOW()),
        ('Pharmacie', 27.90, 'expense', '2026-04-14', 'Medicaments', sante_id, demo_user_id, NOW(), NOW()),
        ('Supermarche semaine 2', 96.15, 'expense', '2026-04-17', 'Courses alimentaires', courses_id, demo_user_id, NOW(), NOW()),
        ('Cinema', 24.00, 'expense', '2026-04-19', 'Sortie weekend', loisirs_id, demo_user_id, NOW(), NOW()),
        ('Veste printemps', 79.99, 'expense', '2026-04-22', 'Vetements', shopping_id, demo_user_id, NOW(), NOW()),
        ('Supermarche semaine 3', 72.60, 'expense', '2026-04-25', 'Courses alimentaires', courses_id, demo_user_id, NOW(), NOW()),

        ('Salaire mars', 2850.00, 'income', '2026-03-01', 'Salaire mensuel', salaire_id, demo_user_id, NOW(), NOW()),
        ('Mission API', 380.00, 'income', '2026-03-12', 'Correction backend', freelance_id, demo_user_id, NOW(), NOW()),
        ('Loyer mars', 920.00, 'expense', '2026-03-03', 'Appartement', logement_id, demo_user_id, NOW(), NOW()),
        ('Courses mars 1', 101.80, 'expense', '2026-03-06', 'Courses alimentaires', courses_id, demo_user_id, NOW(), NOW()),
        ('Essence', 62.30, 'expense', '2026-03-08', 'Deplacement weekend', transport_id, demo_user_id, NOW(), NOW()),
        ('Spotify', 10.99, 'expense', '2026-03-10', 'Abonnement musique', abonnements_id, demo_user_id, NOW(), NOW()),
        ('Dentiste', 68.00, 'expense', '2026-03-15', 'Soin dentaire', sante_id, demo_user_id, NOW(), NOW()),
        ('Restaurant equipe', 37.50, 'expense', '2026-03-18', 'Dejeuner', restaurants_id, demo_user_id, NOW(), NOW()),
        ('Jeu video', 59.99, 'expense', '2026-03-21', 'Loisir', loisirs_id, demo_user_id, NOW(), NOW()),
        ('Courses mars 2', 89.40, 'expense', '2026-03-24', 'Courses alimentaires', courses_id, demo_user_id, NOW(), NOW()),

        ('Salaire fevrier', 2850.00, 'income', '2026-02-01', 'Salaire mensuel', salaire_id, demo_user_id, NOW(), NOW()),
        ('Prime exceptionnelle', 300.00, 'income', '2026-02-14', 'Prime projet', salaire_id, demo_user_id, NOW(), NOW()),
        ('Loyer fevrier', 920.00, 'expense', '2026-02-03', 'Appartement', logement_id, demo_user_id, NOW(), NOW()),
        ('Courses fevrier', 245.70, 'expense', '2026-02-12', 'Courses du mois', courses_id, demo_user_id, NOW(), NOW()),
        ('Train Lyon', 74.00, 'expense', '2026-02-18', 'Billet aller-retour', transport_id, demo_user_id, NOW(), NOW()),
        ('Hotel weekend', 210.00, 'expense', '2026-02-19', 'Weekend court', vacances_id, demo_user_id, NOW(), NOW()),
        ('Restaurant Lyon', 64.80, 'expense', '2026-02-20', 'Diner', restaurants_id, demo_user_id, NOW(), NOW()),

        ('Salaire janvier', 2850.00, 'income', '2026-01-01', 'Salaire mensuel', salaire_id, demo_user_id, NOW(), NOW()),
        ('Mission audit', 540.00, 'income', '2026-01-22', 'Audit technique', freelance_id, demo_user_id, NOW(), NOW()),
        ('Loyer janvier', 920.00, 'expense', '2026-01-03', 'Appartement', logement_id, demo_user_id, NOW(), NOW()),
        ('Courses janvier', 268.45, 'expense', '2026-01-13', 'Courses du mois', courses_id, demo_user_id, NOW(), NOW()),
        ('Soldes hiver', 156.70, 'expense', '2026-01-18', 'Vetements', shopping_id, demo_user_id, NOW(), NOW()),
        ('Abonnements janvier', 36.47, 'expense', '2026-01-21', 'Services mensuels', abonnements_id, demo_user_id, NOW(), NOW()),

        ('Salaire decembre', 2800.00, 'income', '2025-12-01', 'Ancienne periode', salaire_id, demo_user_id, NOW(), NOW()),
        ('Cadeaux Noel', 310.00, 'expense', '2025-12-18', 'Cadeaux famille', shopping_id, demo_user_id, NOW(), NOW()),
        ('Train Noel', 130.00, 'expense', '2025-12-22', 'Retour famille', transport_id, demo_user_id, NOW(), NOW());
END $$;
