using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCSP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent DDL to ensure Wallet tables exist on clean databases
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""Wallets"" (
  ""Id"" uuid NOT NULL,
  ""UserId"" uuid NOT NULL,
  ""Available"" numeric(18,2) NOT NULL DEFAULT 0.0,
  ""Pending"" numeric(18,2) NOT NULL DEFAULT 0.0,
  ""CreatedAt"" timestamptz NOT NULL,
  ""UpdatedAt"" timestamptz NOT NULL,
  CONSTRAINT ""PK_Wallets"" PRIMARY KEY (""Id""),
  CONSTRAINT ""FK_Wallets_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Wallets_UserId"" ON ""Wallets"" (""UserId"");

CREATE TABLE IF NOT EXISTS ""WalletTransactions"" (
  ""Id"" uuid NOT NULL,
  ""WalletId"" uuid NOT NULL,
  ""MomoOrderId"" varchar(100),
  ""MomoRequestId"" varchar(100),
  ""Amount"" numeric(18,2) NOT NULL,
  ""Status"" int NOT NULL,
  ""RawResponse"" text NULL,
  ""CreatedAt"" timestamptz NOT NULL,
  CONSTRAINT ""PK_WalletTransactions"" PRIMARY KEY (""Id""),
  CONSTRAINT ""FK_WalletTransactions_Wallets_WalletId"" FOREIGN KEY (""WalletId"") REFERENCES ""Wallets"" (""Id"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_WalletTransactions_MomoOrderId"" ON ""WalletTransactions"" (""MomoOrderId"");
CREATE INDEX IF NOT EXISTS ""IX_WalletTransactions_MomoRequestId"" ON ""WalletTransactions"" (""MomoRequestId"");
CREATE INDEX IF NOT EXISTS ""IX_WalletTransactions_WalletId"" ON ""WalletTransactions"" (""WalletId"");

CREATE TABLE IF NOT EXISTS ""LedgerEntries"" (
  ""Id"" uuid NOT NULL,
  ""WalletId"" uuid NOT NULL,
  ""Type"" int NOT NULL,
  ""Amount"" numeric(18,2) NOT NULL,
  ""RefId"" varchar(100),
  ""CreatedAt"" timestamptz NOT NULL,
  CONSTRAINT ""PK_LedgerEntries"" PRIMARY KEY (""Id""),
  CONSTRAINT ""FK_LedgerEntries_Wallets_WalletId"" FOREIGN KEY (""WalletId"") REFERENCES ""Wallets"" (""Id"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_LedgerEntries_WalletId_CreatedAt"" ON ""LedgerEntries"" (""WalletId"", ""CreatedAt"");

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS ""LedgerEntries"" CASCADE;
DROP TABLE IF EXISTS ""WalletTransactions"" CASCADE;
DROP TABLE IF EXISTS ""Wallets"" CASCADE;
");
        }
    }
}
