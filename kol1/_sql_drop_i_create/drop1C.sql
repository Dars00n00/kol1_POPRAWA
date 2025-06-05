-- foreign keys
ALTER TABLE k1r_Delivery DROP CONSTRAINT k1r_Delivery_Customer;

ALTER TABLE k1r_Delivery DROP CONSTRAINT k1r_Delivery_Driver;

ALTER TABLE k1r_Product_Delivery DROP CONSTRAINT k1r_Product_Delivery_Delivery;

ALTER TABLE k1r_Product_Delivery DROP CONSTRAINT k1r_Product_Delivery_Product;

-- tables
DROP TABLE k1r_Customer;

DROP TABLE k1r_Delivery;

DROP TABLE k1r_Driver;

DROP TABLE k1r_Product;

DROP TABLE k1r_Product_Delivery;

-- End of file.
