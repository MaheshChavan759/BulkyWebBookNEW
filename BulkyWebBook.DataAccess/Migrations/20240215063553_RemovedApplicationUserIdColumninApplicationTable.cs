﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyWebBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedApplicationUserIdColumninApplicationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationuserId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationuserId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }
    }
}
