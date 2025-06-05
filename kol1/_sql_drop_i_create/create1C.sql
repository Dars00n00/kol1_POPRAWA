-- Table: k1r_Customer
CREATE TABLE k1r_Customer (
    customer_id int NOT NULL,
    first_name nvarchar(100) NOT NULL,
    last_name nvarchar(100) NOT NULL,
    date_of_birth datetime NOT NULL,
    CONSTRAINT k1r_Customer_pk PRIMARY KEY (customer_id)
);

-- Table: k1r_Driver
CREATE TABLE k1r_Driver (
    driver_id int NOT NULL,
    first_name nvarchar(100) NOT NULL,
    last_name nvarchar(100) NOT NULL,
    licence_number nvarchar(17) NOT NULL,
    CONSTRAINT k1r_Driver_pk PRIMARY KEY (driver_id)
);

-- Table: k1r_Product
CREATE TABLE k1r_Product (
    product_id int NOT NULL,
    name nvarchar(100) NOT NULL,
    price decimal(10,2) NOT NULL,
    CONSTRAINT k1r_Product_pk PRIMARY KEY (product_id)
);

-- Table: k1r_Delivery
CREATE TABLE k1r_Delivery (
    delivery_id int NOT NULL,
    customer_id int NOT NULL,
    driver_id int NOT NULL,
    date datetime NOT NULL,
    CONSTRAINT k1r_Delivery_pk PRIMARY KEY (delivery_id)
);

-- Table: k1r_Product_Delivery
CREATE TABLE k1r_Product_Delivery (
    product_id int NOT NULL,
    delivery_id int NOT NULL,
    amount int NOT NULL,
    CONSTRAINT k1r_Product_Delivery_pk PRIMARY KEY (product_id, delivery_id)
);

-- Foreign keys
ALTER TABLE k1r_Delivery ADD CONSTRAINT k1r_Delivery_Customer
    FOREIGN KEY (customer_id)
    REFERENCES k1r_Customer (customer_id);

ALTER TABLE k1r_Delivery ADD CONSTRAINT k1r_Delivery_Driver
    FOREIGN KEY (driver_id)
    REFERENCES k1r_Driver (driver_id);

ALTER TABLE k1r_Product_Delivery ADD CONSTRAINT k1r_Product_Delivery_Delivery
    FOREIGN KEY (delivery_id)
    REFERENCES k1r_Delivery (delivery_id);

ALTER TABLE k1r_Product_Delivery ADD CONSTRAINT k1r_Product_Delivery_Product
    FOREIGN KEY (product_id)
    REFERENCES k1r_Product (product_id);

-- Insert data into k1r_Customer
INSERT INTO k1r_Customer (customer_id, first_name, last_name, date_of_birth) VALUES
(1, 'Emily', 'Clark', '1988-03-12'),
(2, 'Michael', 'Thompson', '1992-11-05'),
(3, 'Sophia', 'Martinez', '1985-07-23');

-- Insert data into k1r_Driver
INSERT INTO k1r_Driver (driver_id, first_name, last_name, licence_number) VALUES
(1, 'James', 'Wright', 'LIC1234567890001'),
(2, 'Olivia', 'Scott', 'LIC1234567890002'),
(3, 'Ethan', 'Lewis', 'LIC1234567890003');

-- Insert data into k1r_Product
INSERT INTO k1r_Product (product_id, name, price) VALUES
(1, 'Smartphone', 699.99),
(2, 'Laptop', 999.99),
(3, 'Headphones', 199.99),
(4, 'Keyboard', 89.50);

-- Insert data into k1r_Delivery
INSERT INTO k1r_Delivery (delivery_id, customer_id, driver_id, date) VALUES
(1, 1, 1, '2024-06-15 14:00:00'),
(2, 2, 2, '2024-06-16 10:30:00'),
(3, 3, 3, '2024-06-17 16:45:00'),
(4, 1, 2, '2024-06-18 09:20:00');

-- Insert data into k1r_Product_Delivery
INSERT INTO k1r_Product_Delivery (product_id, delivery_id, amount) VALUES
(1, 1, 1), -- Smartphone delivered to Emily
(3, 1, 2), -- Headphones delivered to Emily
(2, 2, 1), -- Laptop delivered to Michael
(4, 3, 1), -- Keyboard delivered to Sophia
(1, 4, 1), -- Smartphone delivered again to Emily
(2, 4, 1); -- Laptop delivered again to Emily
