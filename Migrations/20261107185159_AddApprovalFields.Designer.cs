using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PurchaseApp.Data;

#nullable disable

namespace PurchaseApp.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261107185159_AddApprovalFields")]
    partial class AddApprovalFields
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PurchaseApp.Models.PurchaseRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("ApplicantName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Department")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("DesiredDeliveryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDepartmentHeadApproved")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDirectorApproved")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsFinanceApproved")
                        .HasColumnType("boolean");

                    b.Property<string>("ProposedSupplier")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PurchaseOrderNumber")
                        .HasColumnType("text");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("PurchaseRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
