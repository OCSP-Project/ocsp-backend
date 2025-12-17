START TRANSACTION;

ALTER TABLE "Contractors" ADD "GoogleMapsRating" numeric;

ALTER TABLE "Contractors" ADD "GoogleMapsReviewCount" integer;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251217073410_AddMissingGoogleMapsFieldsToContractor', '8.0.0');

COMMIT;

