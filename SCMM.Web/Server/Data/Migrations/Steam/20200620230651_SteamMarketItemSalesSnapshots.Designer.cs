﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SCMM.Web.Server.Data;

namespace SCMM.Web.Server.Data.Migrations.Steam
{
    [DbContext(typeof(SteamDbContext))]
    [Migration("20200620230651_SteamMarketItemSalesSnapshots")]
    partial class SteamMarketItemSalesSnapshots
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamApp", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("IconLargeUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IconUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SteamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SteamApps");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamAssetDescription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BackgroundColour")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ForegroundColour")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IconLargeUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IconUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("LastCheckedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SteamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("WorkshopFileId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("WorkshopFileId");

                    b.ToTable("SteamAssetDescriptions");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamAssetWorkshopFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("AcceptedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("CreatorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Favourited")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("LastCheckedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("SteamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Subscriptions")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Views")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("CreatorId");

                    b.ToTable("SteamAssetWorkshopFiles");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamCurrency", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrefixText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SteamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SuffixText")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SteamCurrencies");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamInventoryItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BuyPrice")
                        .HasColumnType("int");

                    b.Property<Guid?>("CurrencyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DescriptionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("SteamId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("DescriptionId");

                    b.HasIndex("OwnerId");

                    b.ToTable("SteamInventoryItems");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamLanguage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SteamId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SteamLanguages");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AllTimeHighestValue")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("AllTimeHighestValueOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("AllTimeLowestValue")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("AllTimeLowestValueOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BuyAskingPrice")
                        .HasColumnType("int");

                    b.Property<int>("BuyNowPrice")
                        .HasColumnType("int");

                    b.Property<int>("BuyNowPriceDelta")
                        .HasColumnType("int");

                    b.Property<Guid?>("CurrencyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Demand")
                        .HasColumnType("int");

                    b.Property<int>("DemandUnique")
                        .HasColumnType("int");

                    b.Property<Guid?>("DescriptionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("First24hrValue")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("FirstSeenOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Last120hrSales")
                        .HasColumnType("int");

                    b.Property<int>("Last120hrValue")
                        .HasColumnType("int");

                    b.Property<int>("Last24hrSales")
                        .HasColumnType("int");

                    b.Property<int>("Last24hrValue")
                        .HasColumnType("int");

                    b.Property<int>("Last336hrSales")
                        .HasColumnType("int");

                    b.Property<int>("Last336hrValue")
                        .HasColumnType("int");

                    b.Property<int>("Last48hrSales")
                        .HasColumnType("int");

                    b.Property<int>("Last48hrValue")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("LastCheckedOrdersOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("LastCheckedSalesOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("ResellPrice")
                        .HasColumnType("int");

                    b.Property<int>("ResellProfit")
                        .HasColumnType("int");

                    b.Property<int>("ResellTax")
                        .HasColumnType("int");

                    b.Property<string>("SteamId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Supply")
                        .HasColumnType("int");

                    b.Property<int>("SupplyUnique")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("DescriptionId");

                    b.ToTable("SteamMarketItems");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItemOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("BuyItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("SellItemId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("BuyItemId");

                    b.HasIndex("SellItemId");

                    b.ToTable("SteamMarketItemOrder");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItemSale", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("ItemId");

                    b.ToTable("SteamMarketItemSale");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamProfile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AvatarLargeUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfileId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SteamId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SteamProfiles");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamStoreItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AppId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CurrencyId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("DescriptionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SteamId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StorePrice")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppId");

                    b.HasIndex("CurrencyId");

                    b.HasIndex("DescriptionId");

                    b.ToTable("SteamStoreItems");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamApp", b =>
                {
                    b.OwnsMany("SCMM.Web.Server.Domain.Models.Steam.SteamAssetFilter", "Filters", b1 =>
                        {
                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<Guid>("SteamAppId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("SteamId")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("Id");

                            b1.HasIndex("SteamAppId");

                            b1.ToTable("SteamAssetFilter");

                            b1.WithOwner()
                                .HasForeignKey("SteamAppId");

                            b1.OwnsOne("SCMM.Web.Server.Data.Types.PersistableStringDictionary", "Options", b2 =>
                                {
                                    b2.Property<Guid>("SteamAssetFilterId")
                                        .HasColumnType("uniqueidentifier");

                                    b2.Property<string>("Serialised")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("SteamAssetFilterId");

                                    b2.ToTable("SteamAssetFilter");

                                    b2.WithOwner()
                                        .HasForeignKey("SteamAssetFilterId");
                                });
                        });
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamAssetDescription", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamApp", "App")
                        .WithMany("Assets")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamAssetWorkshopFile", "WorkshopFile")
                        .WithMany()
                        .HasForeignKey("WorkshopFileId");

                    b.OwnsOne("SCMM.Web.Server.Data.Types.PersistableStringDictionary", "Tags", b1 =>
                        {
                            b1.Property<Guid>("SteamAssetDescriptionId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Serialised")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("SteamAssetDescriptionId");

                            b1.ToTable("SteamAssetDescriptions");

                            b1.WithOwner()
                                .HasForeignKey("SteamAssetDescriptionId");
                        });
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamAssetWorkshopFile", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamApp", "App")
                        .WithMany("WorkshopFiles")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamProfile", "Creator")
                        .WithMany("WorkshopFiles")
                        .HasForeignKey("CreatorId");

                    b.OwnsOne("SCMM.Web.Server.Data.Types.PersistableGraphDataSet", "SubscriptionsGraph", b1 =>
                        {
                            b1.Property<Guid>("SteamAssetWorkshopFileId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("Serialised")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("SteamAssetWorkshopFileId");

                            b1.ToTable("SteamAssetWorkshopFiles");

                            b1.WithOwner()
                                .HasForeignKey("SteamAssetWorkshopFileId");
                        });
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamInventoryItem", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamApp", "App")
                        .WithMany()
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamCurrency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamAssetDescription", "Description")
                        .WithMany()
                        .HasForeignKey("DescriptionId");

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamProfile", "Owner")
                        .WithMany("InventoryItems")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItem", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamApp", "App")
                        .WithMany("MarketItems")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamCurrency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamAssetDescription", "Description")
                        .WithMany()
                        .HasForeignKey("DescriptionId");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItemOrder", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItem", "BuyItem")
                        .WithMany("BuyOrders")
                        .HasForeignKey("BuyItemId");

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItem", "SellItem")
                        .WithMany("SellOrders")
                        .HasForeignKey("SellItemId");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItemSale", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamMarketItem", "Item")
                        .WithMany("SalesHistory")
                        .HasForeignKey("ItemId");
                });

            modelBuilder.Entity("SCMM.Web.Server.Domain.Models.Steam.SteamStoreItem", b =>
                {
                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamApp", "App")
                        .WithMany("StoreItems")
                        .HasForeignKey("AppId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamCurrency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyId");

                    b.HasOne("SCMM.Web.Server.Domain.Models.Steam.SteamAssetDescription", "Description")
                        .WithMany()
                        .HasForeignKey("DescriptionId");
                });
#pragma warning restore 612, 618
        }
    }
}
