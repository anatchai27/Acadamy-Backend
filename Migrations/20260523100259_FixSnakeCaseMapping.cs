using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace academyAPI.Migrations
{
    public partial class FixSnakeCaseMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Drop duplicate columns from users table (CreatedAt, UpdatedAt were added by mistake)
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'tutoring_db' AND TABLE_NAME = 'users' AND COLUMN_NAME = 'CreatedAt');
                SET @sql = IF(@col_exists > 0, 'ALTER TABLE `users` DROP COLUMN `CreatedAt`', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;

                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'tutoring_db' AND TABLE_NAME = 'users' AND COLUMN_NAME = 'UpdatedAt');
                SET @sql = IF(@col_exists > 0, 'ALTER TABLE `users` DROP COLUMN `UpdatedAt`', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;

                -- Drop non-existent columns from teachers table
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'tutoring_db' AND TABLE_NAME = 'teachers' AND COLUMN_NAME = 'CreatedAt');
                SET @sql = IF(@col_exists > 0, 'ALTER TABLE `teachers` DROP COLUMN `CreatedAt`', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;

                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'tutoring_db' AND TABLE_NAME = 'teachers' AND COLUMN_NAME = 'Nickname');
                SET @sql = IF(@col_exists > 0, 'ALTER TABLE `teachers` DROP COLUMN `Nickname`', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;

                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'tutoring_db' AND TABLE_NAME = 'teachers' AND COLUMN_NAME = 'UpdatedAt');
                SET @sql = IF(@col_exists > 0, 'ALTER TABLE `teachers` DROP COLUMN `UpdatedAt`', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
