CREATE DATABASE Part;

CREATE TABLE item (
    id SERIAL PRIMARY KEY NOT NULL,
    item_name VARCHAR(50) NOT NULL,
    parent_item INTEGER REFERENCES item(id),
    cost INTEGER NOT NULL,
    req_date DATE NOT NULL
);

INSERT INTO item (id, item_name, parent_item, cost, req_date) VALUES
(1, 'Item1', null, 500, '2024-02-20'),
(2, 'Sub1', 1, 200, '2024-02-10'),
(3, 'Sub2', 1, 300, '2024-01-05'),
(4, 'Sub3', 2, 300, '2024-01-02'),
(5, 'Sub4', 2, 400, '2024-01-02'),
(6, 'Item2', null, 600, '2024-03-15'),
(7, 'Sub1', 6, 200, '2024-02-25');

CREATE OR REPLACE FUNCTION Get_Total_Cost(item_name_input VARCHAR)
RETURNS INTEGER AS $$
DECLARE
    total_cost INTEGER := 0;
BEGIN
    -- Check if the provided item_name exists in the table
    IF EXISTS (SELECT 1 FROM item WHERE item_name = item_name_input) THEN
        -- If the item has no parent_item, it's a top-level item
        SELECT INTO total_cost
            SUM(cost)
        FROM item
        WHERE item_name = item_name_input
        OR id = (SELECT parent_item FROM item WHERE item_name = item_name_input);

    ELSE
        -- If the item has a parent_item, return null
        total_cost := NULL;
    END IF;

    RETURN total_cost;
END;
$$ LANGUAGE plpgsql;