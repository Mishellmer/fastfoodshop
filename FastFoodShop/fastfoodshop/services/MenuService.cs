using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using fastfoodshop.Data;
using fastfoodshop.Models;

namespace fastfoodshop.Services
{
    // Операции менеджера по управлению меню и стоп-листом.
    public class MenuService
    {
        public List<MenuItem> GetAll()
        {
            using var db = new AppDbContext();
            return db.MenuItems
                .Include(m => m.Category)
                .OrderBy(m => m.Category!.CategoryName)
                .ThenBy(m => m.ItemName)
                .ToList();
        }

        public List<MenuItem> GetAvailable()
        {
            using var db = new AppDbContext();
            return db.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.Category!.CategoryName)
                .ThenBy(m => m.ItemName)
                .ToList();
        }

        public List<Category> GetCategories()
        {
            using var db = new AppDbContext();
            return db.Categories.OrderBy(c => c.CategoryName).ToList();
        }

        public MenuItem Create(string name, decimal price, int categoryId, bool isAvailable)
        {
            Validate(name, price, categoryId);

            using var db = new AppDbContext();
            EnsureCategoryExists(db, categoryId);

            var item = new MenuItem
            {
                ItemName = name.Trim(),
                Price = price,
                CategoryId = categoryId,
                IsAvailable = isAvailable
            };

            db.MenuItems.Add(item);
            db.SaveChanges();
            return item;
        }

        public void Update(int menuItemId, string name, decimal price, int categoryId, bool isAvailable)
        {
            Validate(name, price, categoryId);

            using var db = new AppDbContext();
            EnsureCategoryExists(db, categoryId);

            var item = db.MenuItems.FirstOrDefault(m => m.MenuItemId == menuItemId)
                       ?? throw new InvalidOperationException("Позиция меню не найдена.");

            item.ItemName = name.Trim();
            item.Price = price;
            item.CategoryId = categoryId;
            item.IsAvailable = isAvailable;

            db.SaveChanges();
        }

        // Изменение состояния стоп-листа.
        public void SetAvailability(int menuItemId, bool isAvailable)
        {
            using var db = new AppDbContext();

            var item = db.MenuItems.FirstOrDefault(m => m.MenuItemId == menuItemId)
                       ?? throw new InvalidOperationException("Позиция меню не найдена.");

            item.IsAvailable = isAvailable;
            db.SaveChanges();
        }

        public void Delete(int menuItemId)
        {
            using var db = new AppDbContext();

            var item = db.MenuItems.FirstOrDefault(m => m.MenuItemId == menuItemId)
                       ?? throw new InvalidOperationException("Позиция меню не найдена.");

            db.MenuItems.Remove(item);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException(
                    "Позиция уже используется в заказах, поэтому удалить её нельзя. " +
                    "Переведите позицию в стоп-лист, чтобы снять её с продажи.");
            }
        }

        private static void Validate(string name, decimal price, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Укажите наименование блюда.");

            if (name.Trim().Length > 100)
                throw new InvalidOperationException("Наименование не должно превышать 100 символов.");

            if (price <= 0)
                throw new InvalidOperationException("Цена должна быть больше нуля.");

            if (categoryId <= 0)
                throw new InvalidOperationException("Выберите категорию блюда.");
        }

        private static void EnsureCategoryExists(AppDbContext db, int categoryId)
        {
            if (!db.Categories.Any(c => c.CategoryId == categoryId))
                throw new InvalidOperationException("Выбранная категория не найдена.");
        }
    }
}
