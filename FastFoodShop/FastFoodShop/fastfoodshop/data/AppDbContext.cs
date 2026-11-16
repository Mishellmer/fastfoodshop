using fastfoodshop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace fastfoodshop.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Discount> Discounts { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var config = builder.Build();
                var connectionString = config.GetConnectionString("DefaultConnection");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Справочник ролей
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).HasColumnName("role_id").ValueGeneratedOnAdd();
                entity.Property(e => e.RoleName).HasColumnName("role_name").HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.RoleName).IsUnique().HasDatabaseName("uq_roles_name");
            });

            // 2. Справочник категорий блюд
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).HasColumnName("category_id").ValueGeneratedOnAdd();
                entity.Property(e => e.CategoryName).HasColumnName("category_name").HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.CategoryName).IsUnique().HasDatabaseName("uq_categories_name");
            });

            // 3. Справочник скидок
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discounts");
                entity.HasKey(e => e.DiscountId);
                entity.Property(e => e.DiscountId).HasColumnName("discount_id").ValueGeneratedOnAdd();
                entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent").HasPrecision(5, 2).IsRequired();
                entity.Property(e => e.MinOrderSum).HasColumnName("min_order_sum").HasPrecision(10, 2).IsRequired();
            });

            // 4. Сотрудники
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id").ValueGeneratedOnAdd();
                entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
                entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
                entity.Property(e => e.MiddleName).HasColumnName("middle_name").HasMaxLength(50);
                entity.Property(e => e.Login).HasColumnName("login").HasMaxLength(50).IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
                entity.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();

                entity.HasIndex(e => e.Login).IsUnique().HasDatabaseName("uq_employees_login");

                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .HasConstraintName("fk_employees_roles")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. Позиции меню
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.ToTable("MenuItems");
                entity.HasKey(e => e.MenuItemId);
                entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id").ValueGeneratedOnAdd();
                entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
                entity.Property(e => e.IsAvailable).HasColumnName("is_available").IsRequired().HasDefaultValue(true);

                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .HasConstraintName("fk_menuitems_categories")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 6. Клиенты
            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("Clients");
                entity.HasKey(e => e.ClientId);
                entity.Property(e => e.ClientId).HasColumnName("client_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
                entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
                entity.Property(e => e.DiscountId).HasColumnName("discount_id");

                entity.HasIndex(e => e.Phone).IsUnique().HasDatabaseName("uq_clients_phone");

                entity.HasOne(e => e.Discount)
                    .WithMany()
                    .HasForeignKey(e => e.DiscountId)
                    .HasConstraintName("fk_clients_discounts")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Заказы
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).HasColumnName("order_id").ValueGeneratedOnAdd();
                entity.Property(e => e.OrderDate)
                    .HasColumnName("order_date")
                    .HasColumnType("datetime")
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ClientId).HasColumnName("client_id");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id").IsRequired();
                entity.Property(e => e.TotalSum).HasColumnName("total_sum").HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("новый");

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .HasConstraintName("fk_orders_clients")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .HasConstraintName("fk_orders_employees")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .HasConstraintName("fk_orderitems_orders")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. Позиции заказов
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(e => e.OrderItemId);
                entity.Property(e => e.OrderItemId).HasColumnName("order_item_id").ValueGeneratedOnAdd();
                entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.PriceAtOrder).HasColumnName("price_at_order").HasPrecision(10, 2).IsRequired();

                entity.HasOne(e => e.MenuItem)
                    .WithMany()
                    .HasForeignKey(e => e.MenuItemId)
                    .HasConstraintName("fk_orderitems_menuitems")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
