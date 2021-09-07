﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModelScoutAPI;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ModelScoutAPI.Migrations
{
    [DbContext(typeof(ModelScoutDbContext))]
    partial class ModelScoutDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ModelScoutAPI.Models.MiscInfo", b =>
                {
                    b.Property<int>("MiscInfoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<DateTime>("LastDateOfClearLimits")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("MiscInfoId");

                    b.ToTable("MiscInfos");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<int>("CurrentStep")
                        .HasColumnType("integer");

                    b.Property<long>("LastMessageId")
                        .HasColumnType("bigint");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.VkAcc", b =>
                {
                    b.Property<int>("VkAccId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("AccessToken")
                        .HasColumnType("text");

                    b.Property<int>("AgeFrom")
                        .HasColumnType("integer");

                    b.Property<int>("AgeTo")
                        .HasColumnType("integer");

                    b.Property<int>("BirthDay")
                        .HasColumnType("integer");

                    b.Property<int>("BirthMonth")
                        .HasColumnType("integer");

                    b.Property<int>("City")
                        .HasColumnType("integer");

                    b.Property<int>("CountAddedFriends")
                        .HasColumnType("integer");

                    b.Property<int>("Country")
                        .HasColumnType("integer");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<int>("FriendsLimit")
                        .HasColumnType("integer");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<int>("Sex")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("VkAccStatus")
                        .HasColumnType("integer");

                    b.HasKey("VkAccId");

                    b.HasIndex("UserId");

                    b.ToTable("VkAccs");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.VkClient", b =>
                {
                    b.Property<int>("VkClientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<int>("ClientStatus")
                        .HasColumnType("integer");

                    b.Property<int>("ProfileVkId")
                        .HasColumnType("integer");

                    b.Property<int>("VkAccId")
                        .HasColumnType("integer");

                    b.HasKey("VkClientId");

                    b.HasIndex("VkAccId");

                    b.ToTable("VkClients");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.VkAcc", b =>
                {
                    b.HasOne("ModelScoutAPI.Models.User", "User")
                        .WithMany("VkAccs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.VkClient", b =>
                {
                    b.HasOne("ModelScoutAPI.Models.VkAcc", "VkAcc")
                        .WithMany("VkClients")
                        .HasForeignKey("VkAccId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("VkAcc");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.User", b =>
                {
                    b.Navigation("VkAccs");
                });

            modelBuilder.Entity("ModelScoutAPI.Models.VkAcc", b =>
                {
                    b.Navigation("VkClients");
                });
#pragma warning restore 612, 618
        }
    }
}
