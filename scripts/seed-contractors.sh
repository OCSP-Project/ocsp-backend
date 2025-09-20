#!/usr/bin/env bash
set -euo pipefail

echo "🌱 Seeding contractor data..."

# Chạy script SQL để thêm dữ liệu contractor
docker compose exec -T db psql -U ocsp -d ocsp << 'EOF'
-- Thêm Users cho contractors
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "Role", "IsEmailVerified", "CreatedAt", "UpdatedAt")
VALUES 
    ('550e8400-e29b-41d4-a716-446655440001', 'contractor_test1', 'contractor1@test.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdQvOQ5/9Qd1HYp5x8l6vJQvKzKzK', 'Contractor', true, NOW(), NOW()),
    ('550e8400-e29b-41d4-a716-446655440002', 'contractor_test2', 'contractor2@test.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdQvOQ5/9Qd1HYp5x8l6vJQvKzKzK', 'Contractor', true, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Thêm Contractors
INSERT INTO "Contractors" (
    "Id", "UserId", "CompanyName", "BusinessLicense", "Description", 
    "ContactPhone", "ContactEmail", "Address", "City", "Province",
    "YearsOfExperience", "TeamSize", "IsVerified", "IsActive", "IsPremium",
    "ProfileCompletionPercentage", "CreatedAt", "UpdatedAt"
)
VALUES 
    (
        '650e8400-e29b-41d4-a716-446655440001',
        '550e8400-e29b-41d4-a716-446655440001',
        'Công ty Xây Dựng ABC',
        'BL123456789',
        'Chuyên thi công nhà ở, biệt thự cao cấp với 5 năm kinh nghiệm',
        '0901234567',
        'contractor1@test.com',
        '123 Đường ABC, Quận 1',
        'TP.HCM',
        'TP.HCM',
        5,
        10,
        true,
        true,
        false,
        85,
        NOW(),
        NOW()
    ),
    (
        '650e8400-e29b-41d4-a716-446655440002',
        '550e8400-e29b-41d4-a716-446655440002',
        'Công ty Kiến Trúc XYZ',
        'BL987654321', 
        'Thiết kế và thi công công trình dân dụng, chuyên nghiệp',
        '0907654321',
        'contractor2@test.com',
        '456 Đường XYZ, Quận 2',
        'TP.HCM',
        'TP.HCM',
        8,
        15,
        true,
        true,
        true,
        95,
        NOW(),
        NOW()
    )
ON CONFLICT ("Id") DO NOTHING;

-- Hiển thị kết quả
SELECT 
    c."Id",
    u."Username",
    c."CompanyName",
    c."BusinessLicense",
    c."IsVerified",
    c."IsPremium"
FROM "Contractors" c
JOIN "Users" u ON c."UserId" = u."Id"
ORDER BY c."CreatedAt" DESC;
EOF

echo "✅ Contractor data seeded successfully!"



