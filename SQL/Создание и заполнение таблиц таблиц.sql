-- Создание базы данных
CREATE DATABASE FastFoodDB;
GO

USE FastFoodDB;
GO

-- 1. Справочник ролей
CREATE TABLE Roles (
    role_id    int IDENTITY(1,1) NOT NULL,
    role_name  nvarchar(50) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (role_id),
    CONSTRAINT uq_roles_name UNIQUE (role_name)
);
GO

-- 2. Справочник категорий блюд
CREATE TABLE Categories (
    category_id    int IDENTITY(1,1) NOT NULL,
    category_name  nvarchar(50) NOT NULL,
    CONSTRAINT pk_categories PRIMARY KEY (category_id),
    CONSTRAINT uq_categories_name UNIQUE (category_name)
);
GO

-- 3. Справочник скидок
CREATE TABLE Discounts (
    discount_id      int IDENTITY(1,1) NOT NULL,
    discount_percent decimal(5,2) NOT NULL,
    min_order_sum    decimal(10,2) NOT NULL,
    CONSTRAINT pk_discounts PRIMARY KEY (discount_id),
    CONSTRAINT ck_discounts_percent CHECK (discount_percent BETWEEN 0 AND 100),
    CONSTRAINT ck_discounts_sum CHECK (min_order_sum >= 0)
);
GO

-- 4. Сотрудники
CREATE TABLE Employees (
    employee_id   int IDENTITY(1,1) NOT NULL,
    last_name     nvarchar(50) NOT NULL,
    first_name    nvarchar(50) NOT NULL,
    middle_name   nvarchar(50) NULL,
    login         nvarchar(50) NOT NULL,
    password_hash nvarchar(256) NOT NULL,
    role_id       int NOT NULL,
    CONSTRAINT pk_employees PRIMARY KEY (employee_id),
    CONSTRAINT uq_employees_login UNIQUE (login),
    CONSTRAINT fk_employees_roles FOREIGN KEY (role_id)
        REFERENCES Roles (role_id)
);
GO

-- 5. Позиции меню
CREATE TABLE MenuItems (
    menu_item_id int IDENTITY(1,1) NOT NULL,
    item_name    nvarchar(100) NOT NULL,
    price        decimal(10,2) NOT NULL,
    category_id  int NOT NULL,
    is_available bit NOT NULL CONSTRAINT df_menuitems_available DEFAULT 1,
    CONSTRAINT pk_menuitems PRIMARY KEY (menu_item_id),
    CONSTRAINT ck_menuitems_price CHECK (price > 0),
    CONSTRAINT fk_menuitems_categories FOREIGN KEY (category_id)
        REFERENCES Categories (category_id)
);
GO

-- 6. Клиенты
CREATE TABLE Clients (
    client_id   int IDENTITY(1,1) NOT NULL,
    phone       nvarchar(20) NOT NULL,
    last_name   nvarchar(50) NOT NULL,
    first_name  nvarchar(50) NOT NULL,
    discount_id int NULL,
    CONSTRAINT pk_clients PRIMARY KEY (client_id),
    CONSTRAINT uq_clients_phone UNIQUE (phone),
    CONSTRAINT fk_clients_discounts FOREIGN KEY (discount_id)
        REFERENCES Discounts (discount_id)
);
GO

-- 7. Заказы
CREATE TABLE Orders (
    order_id    int IDENTITY(1,1) NOT NULL,
    order_date  datetime NOT NULL CONSTRAINT df_orders_date DEFAULT GETDATE(),
    client_id   int NULL,
    employee_id int NOT NULL,
    total_sum   decimal(10,2) NOT NULL,
    status      nvarchar(20) NOT NULL CONSTRAINT df_orders_status DEFAULT N'новый',
    CONSTRAINT pk_orders PRIMARY KEY (order_id),
    CONSTRAINT ck_orders_sum CHECK (total_sum >= 0),
    CONSTRAINT ck_orders_status CHECK (status IN (N'новый', N'оплачен', N'выполнен', N'отменён')),
    CONSTRAINT fk_orders_clients FOREIGN KEY (client_id)
        REFERENCES Clients (client_id),
    CONSTRAINT fk_orders_employees FOREIGN KEY (employee_id)
        REFERENCES Employees (employee_id)
);
GO

-- 8. Позиции заказа
CREATE TABLE OrderItems (
    order_item_id  int IDENTITY(1,1) NOT NULL,
    order_id       int NOT NULL,
    menu_item_id   int NOT NULL,
    quantity       int NOT NULL,
    price_at_order decimal(10,2) NOT NULL,
    CONSTRAINT pk_orderitems PRIMARY KEY (order_item_id),
    CONSTRAINT ck_orderitems_qty CHECK (quantity > 0),
    CONSTRAINT ck_orderitems_price CHECK (price_at_order > 0),
    CONSTRAINT fk_orderitems_orders FOREIGN KEY (order_id)
        REFERENCES Orders (order_id) ON DELETE CASCADE,
    CONSTRAINT fk_orderitems_menuitems FOREIGN KEY (menu_item_id)
        REFERENCES MenuItems (menu_item_id)
);
GO


-- Справочники
INSERT INTO Roles (role_name) VALUES
    (N'Кассир'),
    (N'Менеджер'),
    (N'Администратор');

INSERT INTO Categories (category_name) VALUES
    (N'Бургеры'),
    (N'Напитки'),
    (N'Десерты'),
    (N'Картофель и закуски'),
    (N'Салаты');

INSERT INTO Discounts (discount_percent, min_order_sum) VALUES
    (5.00, 500.00),
    (10.00, 1000.00),
    (0.00, 0.00);

-- Сотрудники
INSERT INTO Employees (last_name, first_name, middle_name, login, password_hash, role_id) VALUES
    (N'Владимирович', N'Александр', N'Геннадьевич', N'admin',  N'admin123',  3),
    (N'Соколов', N'Максим', N'Андреевич', N'manager', N'manager123', 2),
    (N'Иванов',   N'Иван',   N'Иванович',    N'cashier1', N'cashier123',    1),
    (N'Петрова',  N'Анна',   N'Сергеевна',   N'cashier2', N'cashier123',    1),
    (N'Сидоров',  N'Пётр',   N'Алексеевич',  N'cashier3', N'cashier123',    1);

-- Позиции меню
INSERT INTO MenuItems (item_name, price, category_id, is_available) VALUES
    (N'Гамбургер классический', 149.00, 1, 1),
    (N'Чизбургер',              179.00, 1, 1),
    (N'Двойной чизбургер',      249.00, 1, 1),
    (N'Куриный бургер',         199.00, 1, 1),
    (N'Вегетарианский бургер',  189.00, 1, 0),
    (N'Кола 0,5 л',              99.00, 2, 1),
    (N'Спрайт 0,5 л',            99.00, 2, 1),
    (N'Сок апельсиновый',       129.00, 2, 1),
    (N'Кофе американо',         119.00, 2, 1),
    (N'Молочный коктейль',      169.00, 2, 1),
    (N'Мороженое ванильное',     89.00, 3, 1),
    (N'Пирожок вишнёвый',        79.00, 3, 1),
    (N'Чизкейк',                159.00, 3, 1),
    (N'Картофель фри малый',     89.00, 4, 1),
    (N'Картофель фри большой',  129.00, 4, 1),
    (N'Наггетсы 6 шт.',         159.00, 4, 1),
    (N'Кольца луковые',         119.00, 4, 1),
    (N'Салат Цезарь',           219.00, 5, 1),
    (N'Салат овощной',          139.00, 5, 1),
    (N'Салат с тунцом',         239.00, 5, 0);

-- Клиенты
INSERT INTO Clients (phone, last_name, first_name, discount_id) VALUES
    (N'+79001112233', N'Смирнов',  N'Алексей',  1),
    (N'+79002223344', N'Кузнецова',N'Мария',    1),
    (N'+79003334455', N'Попов',    N'Дмитрий',  2),
    (N'+79004445566', N'Волкова',  N'Елена',    1),
    (N'+79005556677', N'Соколов',  N'Андрей',   3),
    (N'+79006667788', N'Лебедева', N'Ольга',    3),
    (N'+79007778899', N'Козлов',   N'Сергей',   1),
    (N'+79008889900', N'Новикова', N'Татьяна',  2),
    (N'+79009990011', N'Морозов',  N'Николай',  3),
    (N'+79001001122', N'Павлова',  N'Ирина',    1);

-- Заказы
INSERT INTO Orders (order_date, client_id, employee_id, total_sum, status) VALUES
    ('2026-09-01 12:15:00', 1,    3, 527.00,  N'выполнен'),
    ('2026-09-01 12:40:00', NULL, 3, 248.00,  N'выполнен'),
    ('2026-09-01 13:05:00', 2,    4, 896.00,  N'выполнен'),
    ('2026-09-01 13:30:00', NULL, 5, 199.00,  N'выполнен'),
    ('2026-09-01 14:00:00', 3,    3, 1120.00, N'оплачен'),
    ('2026-09-02 11:20:00', 4,    4, 358.00,  N'выполнен'),
    ('2026-09-02 12:00:00', NULL, 5, 478.00,  N'выполнен'),
    ('2026-09-02 12:45:00', 5,    3, 267.00,  N'выполнен'),
    ('2026-09-02 13:10:00', 6,    4, 740.00,  N'отменён'),
    ('2026-09-02 14:30:00', 7,    5, 545.00,  N'выполнен');


-- Позиции заказов
INSERT INTO OrderItems (order_id, menu_item_id, quantity, price_at_order) VALUES
    (1, 1,  1, 149.00),
    (1, 14, 1,  89.00),
    (1, 6,  1,  99.00),
    (1, 11, 1,  89.00),
    (1, 1,  1, 149.00),
    (2, 2,  1, 179.00),
    (2, 6,  1,  99.00),
    (3, 3,  1, 249.00),
    (3, 15, 1, 129.00),
    (3, 8,  2, 129.00),
    (3, 13, 1, 159.00),
    (4, 4,  1, 199.00),
    (5, 3,  2, 249.00),
    (5, 16, 1, 159.00),
    (5, 7,  2,  99.00),
    (5, 11, 1,  89.00),
    (5, 18, 1, 219.00);