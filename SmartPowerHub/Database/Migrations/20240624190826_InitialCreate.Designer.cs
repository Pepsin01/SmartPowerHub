﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartPowerHub.Database.Contexts;

#nullable disable

namespace SmartPowerHub.Database.Migrations
{
    [DbContext(typeof(ApplianceContext))]
    [Migration("20240624190826_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("SmartPowerHub.Database.Models.ApplianceModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Configuration")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ControllerName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Appliance");
                });
#pragma warning restore 612, 618
        }
    }
}
