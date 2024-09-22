using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tech_Shop.Models;

namespace Tech_Shop.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "Data Source=DESKTOP-5PF17TM\\SQLEXPRESS;Initial Catalog=computershopdb;Integrated Security=True;";
        private computershopdbEntities db = new computershopdbEntities();

        // Index action
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ComingSoon()
        {
            return View();
        }
        public ActionResult SearchResults()
        {


            return View();
        }

        // Testout action to display a random product
        public ActionResult Testout()
        {
            // Fetch a random product from the c_p table
            var randomProduct = db.c_p.OrderBy(r => System.Guid.NewGuid()).FirstOrDefault();

            // Pass the random product to the view
            return View(randomProduct);
        }

       /*------------------------------ Get imgage--------------------*/


        public ActionResult GetImage(string filename)
        {
            string filePath = Server.MapPath("~/App_Data/DB_IMG/" + filename);
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound(); // Handle the case where the image does not exist
            }
            return File(filePath, "image/png");



        }
       /* ---------------------------------Error---------------------*/

        public ActionResult Error()
        {
            return View();
        }







        public ActionResult ComingSoon2()
        {
            return View();
        }







        /*------------------------------------- Search-----------------------------------------------*/


        public ActionResult Search(string query)
        {
            // Allow searches for one character or more
            if (!string.IsNullOrEmpty(query)) // No length check
            {
                string sanitizedQuery = query.Trim();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // First, check for an exact match
                    string exactMatchQuery = "SELECT ID FROM c_p WHERE LOWER(Name) = @query";
                    SqlCommand exactMatchCmd = new SqlCommand(exactMatchQuery, conn);
                    exactMatchCmd.Parameters.AddWithValue("@query", sanitizedQuery.ToLower());

                    var exactMatchId = exactMatchCmd.ExecuteScalar();
                    if (exactMatchId != null)
                    {
                        // Redirect to the individual product page if an exact match is found
                        return RedirectToAction("Individual_product", "Home", new { id = (int)exactMatchId });
                    }

                    // Proceed with a partial match search
                    string sqlQuery = "SELECT ID, Name, ImagePath, Price FROM c_p WHERE LOWER(Name) LIKE @query";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@query", "%" + sanitizedQuery.ToLower() + "%");

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<int> productIds = new List<int>();
                    List<string> productNames = new List<string>();
                    List<string> productImages = new List<string>();
                    List<decimal> productPrices = new List<decimal>();

                    while (reader.Read())
                    {
                        productIds.Add(reader.GetInt32(0));
                        productNames.Add(reader.GetString(1));
                        productImages.Add(reader.GetString(2)); // Assuming ImagePath is the third column
                        productPrices.Add(reader.GetDecimal(3)); // Assuming Price is the fourth column
                    }

                    if (productIds.Count > 0)
                    {
                        // Store product data in ViewBag for the search results page
                        ViewBag.ProductIds = productIds;
                        ViewBag.ProductNames = productNames;
                        ViewBag.ProductImages = productImages;
                        ViewBag.ProductPrices = productPrices;
                        ViewBag.Query = sanitizedQuery;

                        return View("SearchResults"); // Redirect to the results view
                    }
                }
            }

            ViewBag.Message = "No product found for your search.";
            return View("SearchResults"); // Show message if no products found
        }





























        /*-------------------------------------------Homepage--------------------------------------*/
        public ActionResult Homepage()
        {
            List<c_p> popularProducts = new List<c_p>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT TOP 6 * FROM c_p
            WHERE clickcounter IS NOT NULL
            ORDER BY clickcounter DESC";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    popularProducts.Add(new c_p
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Availability = reader["Availability"].ToString(),
                        Brand = reader["Brand"].ToString(),
                        Price = (decimal)reader["Price"],
                        ImagePath = reader["ImagePath"].ToString(),
                        product_id = reader["product_id"] as int?,
                        clickcounter = reader["clickcounter"] as int?
                    });
                }
            }

            ViewBag.PopularProducts = popularProducts;
            return View();
        }













        /*-------------------------------------------About--------------------------------------------*/
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

















        /*-------------------------------------------Contact--------------------------------------*/
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitContact(contact model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                INSERT INTO contact (name, email, subject, message, created_at)
                VALUES (@name, @Email, @Subject, @Message, @CreatedAt)";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@name", model.name);
                    cmd.Parameters.AddWithValue("@Email", model.email);
                    cmd.Parameters.AddWithValue("@Subject", model.subject);
                    cmd.Parameters.AddWithValue("@Message", model.message);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Contact");
            }

            return View("Contact", model);
        }












        /*---------------------------------------------LOgin------------------------------*/


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            user userDetails = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM users WHERE email = @Email AND password = @Password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    userDetails = new user
                    {
                        user_id = (int)reader["user_id"],
                        username = reader["username"].ToString(),
                        email = reader["email"].ToString(),
                        password = reader["password"].ToString(),
                        admin = reader["admin"].ToString(),
                        name = reader["name"].ToString(),
                        phone = reader["phone"].ToString()
                    };
                }
            }

            if (userDetails != null)
            {
                // Store user info in session
                Session["user"] = userDetails;

                string isAdmin = userDetails.admin?.Trim().ToUpper();
                if (string.Equals(isAdmin, "YES", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("AdminDashboard");
                }
                else
                {
                    return RedirectToAction("Homepage");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            // Clear the session
            Session.Clear();

            // Redirect to the Login page
            return RedirectToAction("Login");
        }




















        /*-------------------------------------------AdminDashBoard--------------------------------------*/
        public ActionResult AdminDashboard()
        {
            List<c_p> products = new List<c_p>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM c_p";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    products.Add(new c_p
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Availability = reader["Availability"].ToString(),
                        Brand = reader["Brand"].ToString(),
                        Price = (decimal)reader["Price"],
                        ImagePath = reader["ImagePath"].ToString(),
                    });
                }
            }
            ViewBag.Products = products;
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(string Name, string Availability, string Brand, decimal Price, HttpPostedFileBase productImage)
        {
            if (Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be a positive number.");
                return RedirectToAction("AdminDashboard"); // Optionally return the form with the error message
            }

            string imagePath = null;
            string uploadDir = Server.MapPath("~/App_Data/DB_IMG/");

            // Ensure the directory exists
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Save the image if provided
            if (productImage != null && productImage.ContentLength > 0)
            {
                string fileName = Path.GetFileName(productImage.FileName);
                imagePath = Path.Combine("~/App_Data/DB_IMG/", fileName); // Virtual path to store in the DB
                string physicalPath = Path.Combine(uploadDir, fileName);  // Physical path to save the file
                productImage.SaveAs(physicalPath);
            }

            int newProductId = 1000;

            // Determine the next product_id value (start at 1000 and increment)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Query to find the maximum product_id
                string getMaxProductIdQuery = "SELECT ISNULL(MAX(product_id), 999) FROM c_p";
                SqlCommand getMaxProductIdCmd = new SqlCommand(getMaxProductIdQuery, conn);
                var result = getMaxProductIdCmd.ExecuteScalar();

                // Increment the product_id from the maximum value
                if (result != DBNull.Value)
                {
                    newProductId = Convert.ToInt32(result) + 1;
                }
            }

            // Insert the new product with the new product_id
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO c_p (Name, Availability, Brand, Price, ImagePath, product_id) " +
                               "VALUES (@Name, @Availability, @Brand, @Price, @ImagePath, @ProductId)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@Availability", Availability);
                cmd.Parameters.AddWithValue("@Brand", Brand);
                cmd.Parameters.AddWithValue("@Price", Price);
                cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductId", newProductId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("AdminDashboard");
        }






        [HttpPost]
        public ActionResult UpdateProduct(int ID, string Name, string Availability, string Brand, decimal Price, HttpPostedFileBase productImage)
        {
            if (Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be a positive number.");
                return RedirectToAction("AdminDashboard"); // Optionally return the form with the error message
            }

            string imagePath = null;
            string uploadDir = Server.MapPath("~/App_Data/DB_IMG/");

            // Retrieve the existing image path if no new image is uploaded
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string getImageQuery = "SELECT ImagePath FROM c_p WHERE ID = @ID";
                SqlCommand getImageCmd = new SqlCommand(getImageQuery, conn);
                getImageCmd.Parameters.AddWithValue("@ID", ID);
                conn.Open();
                var result = getImageCmd.ExecuteScalar();
                imagePath = result != DBNull.Value ? result.ToString() : null;
            }

            // If a new image is uploaded, save it
            if (productImage != null && productImage.ContentLength > 0)
            {
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir); // Ensure directory exists
                }
                string fileName = Path.GetFileName(productImage.FileName);
                imagePath = Path.Combine("~/App_Data/DB_IMG/", fileName); // Virtual path to store in the DB
                string physicalPath = Path.Combine(uploadDir, fileName);  // Physical path to save the file
                productImage.SaveAs(physicalPath);
            }

            // Database update operation (note: product_id is NOT updated here)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE c_p SET Name = @Name, Availability = @Availability, Brand = @Brand, Price = @Price, ImagePath = @ImagePath WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@Availability", Availability);
                cmd.Parameters.AddWithValue("@Brand", Brand);
                cmd.Parameters.AddWithValue("@Price", Price);
                cmd.Parameters.AddWithValue("@ImagePath", (object)imagePath ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("AdminDashboard");
        }






        public ActionResult DeleteProduct(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM c_p WHERE ID = @ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("AdminDashboard");
        }




















    /*-------------------------------------------Registration--------------------------------------*/
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(string username, string email, string password, string confirmPassword, string fullname, string phone)
        {
            // Check if passwords match
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(); // Return to registration form with error
            }

            // Insert into the database
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO [users] (username, email, password, admin, name, phone) 
                                 VALUES (@Username, @Email, @Password, 'NO', @Name, @Phone)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Name", fullname);
                cmd.Parameters.AddWithValue("@Phone", phone);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Redirect to a success page or login
            return RedirectToAction("Login");
        }
        // Process the registration form submission





















        /*-------------------------------------------prebuild--------------------------------------*/
        public ActionResult Prebuilt()
        {

            return View();
        }


















        /*--------------------------------- products------------------------*/
        public ActionResult Products(string availability = "", decimal? priceMin = null, decimal? priceMax = null, string sortOrder = "")
        {
            List<c_p> products = new List<c_p>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM c_p WHERE 1=1";

                // Filter by availability
                if (!string.IsNullOrEmpty(availability))
                {
                    if (availability == "in-stock")
                    {
                        query += " AND Availability = 'YES'";
                    }
                    else if (availability == "out-of-stock")
                    {
                        query += " AND Availability = 'NO'";
                    }
                }

                // Filter by price range
                if (priceMin.HasValue)
                {
                    query += " AND Price >= @priceMin";
                }
                if (priceMax.HasValue)
                {
                    query += " AND Price <= @priceMax";
                }

                // Sorting
                if (sortOrder == "low-to-high")
                {
                    query += " ORDER BY Price ASC";
                }
                else if (sortOrder == "high-to-low")
                {
                    query += " ORDER BY Price DESC";
                }

                SqlCommand cmd = new SqlCommand(query, conn);

                if (priceMin.HasValue)
                {
                    cmd.Parameters.AddWithValue("@priceMin", priceMin);
                }
                if (priceMax.HasValue)
                {
                    cmd.Parameters.AddWithValue("@priceMax", priceMax);
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    products.Add(new c_p
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Availability = reader["Availability"].ToString(),
                        Brand = reader["Brand"].ToString(),
                        Price = (decimal)reader["Price"],
                        ImagePath = reader["ImagePath"].ToString(),
                        product_id = reader["product_id"] as int?
                    });
                }
            }

            return View(products);
        }


















        /*-------------------------------------------Feedback--------------------------------------*/

        public ActionResult Feedback()
        {
            List<contact> contacts = new List<contact>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM contact ORDER BY id DESC";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    contacts.Add(new contact
                    {
                        id = (int)reader["id"],
                        name = reader["name"].ToString(),
                        email = reader["email"].ToString(),
                        subject = reader["subject"].ToString(),
                        message = reader["message"].ToString(),
                        created_at = (DateTime)reader["created_at"]
                    });
                }
            }

            return View(contacts); // Pass the contacts to the view
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }








        /* ------------------------------IndividuaProduct---------------------------*/
        public ActionResult Individual_product(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("ErrorPage"); // Handle the case when id is null
            }

            c_p product = null;
            List<c_p> relatedProducts = new List<c_p>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get the selected product using ID instead of product_id
                string query = "SELECT * FROM c_p WHERE ID = @id"; // Fetch by ID, not product_id
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id.Value);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    product = new c_p
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Availability = reader["Availability"].ToString(),
                        Brand = reader["Brand"].ToString(),
                        Price = (decimal)reader["Price"],
                        ImagePath = reader["ImagePath"].ToString(),
                        product_id = (int)reader["product_id"],
                        clickcounter = reader["clickcounter"] as int? ?? 0
                    };
                }
                reader.Close();

                // If no product is found, handle it appropriately
                if (product == null)
                {
                    return RedirectToAction("ErrorPage");
                }

                // Fetch related products based on product_id range
                string relatedProductsQuery = "SELECT * FROM c_p WHERE product_id BETWEEN @minId AND @maxId AND ID != @id";
                SqlCommand relatedCmd = new SqlCommand(relatedProductsQuery, conn);
                relatedCmd.Parameters.AddWithValue("@minId", product.product_id - 5);
                relatedCmd.Parameters.AddWithValue("@maxId", product.product_id + 5);
                relatedCmd.Parameters.AddWithValue("@id", product.ID); // Use ID to exclude the current product

                SqlDataReader relatedReader = relatedCmd.ExecuteReader();
                while (relatedReader.Read())
                {
                    relatedProducts.Add(new c_p
                    {
                        ID = (int)relatedReader["ID"],
                        Name = relatedReader["Name"].ToString(),
                        Availability = relatedReader["Availability"].ToString(),
                        Brand = relatedReader["Brand"].ToString(),
                        Price = (decimal)relatedReader["Price"],
                        ImagePath = relatedReader["ImagePath"].ToString(),
                        product_id = (int)relatedReader["product_id"]
                    });
                }
                relatedReader.Close();
            }

            // Pass the product and related products to the view
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }































        /*-------------------------------------------Revenue--------------------------------------*/
        public ActionResult Revenue()
        {
            ViewBag.TotalRevenue = TempData["TotalRevenue"] ?? 0;
            return View();
        }

        public ActionResult Checkout()
        {
            int userId = 1; // Replace with the logic to get the current user's ID
            decimal totalAmount = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT c.quantity, p.Price
            FROM cart c
            INNER JOIN c_p p ON c.c_p_id = p.ID
            WHERE c.user_id = @userId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    totalAmount += (decimal)reader["Price"] * (int)reader["quantity"];
                }

                // Clear the cart for the user after checkout
                string clearCartQuery = "DELETE FROM cart WHERE user_id = @userId";
                SqlCommand clearCartCmd = new SqlCommand(clearCartQuery, conn);
                clearCartCmd.Parameters.AddWithValue("@userId", userId);
                clearCartCmd.ExecuteNonQuery();
            }

            // Redirect to Revenue page and pass the total amount
            TempData["TotalRevenue"] = totalAmount;
            return RedirectToAction("Revenue");
        }





























        /*---------------------------------------Individual_prebuilds--------------------------------------*/
        public ActionResult Individual_prebuilds()
        {

            return View();
        }






















        /*-------------------------------------------cart--------------------------------------*/
        public ActionResult cart()
        {
            int userId = 1; // Replace with the actual logic to get the current user's ID
            List<cart> userCartItems = new List<cart>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT c.cart_id, c.quantity, c.c_p_id, c.user_id, p.ID, p.Name, p.Price, p.ImagePath
            FROM cart c
            INNER JOIN c_p p ON c.c_p_id = p.ID
            WHERE c.user_id = @userId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cart cartItem = new cart
                    {
                        cart_id = (int)reader["cart_id"],
                        quantity = (int)reader["quantity"],
                        c_p_id = (int)reader["c_p_id"],
                        user_id = (int)reader["user_id"],
                        c_p = new c_p
                        {
                            ID = (int)reader["ID"],
                            Name = reader["Name"].ToString(),
                            Price = (decimal)reader["Price"],
                            ImagePath = reader["ImagePath"].ToString()
                        }
                    };
                    userCartItems.Add(cartItem);
                }

                reader.Close();
            }

            return View(userCartItems);
        }

        private int GetCurrentUserId()
        {
            // Example logic to get the current user's ID.
            // Replace this with your actual logic, e.g., from session, claims, or database.
            return (int)(Session["UserId"] ?? 0);
        }


        public ActionResult AddToCart(int id, int quantity = 1)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get product by ID
                string query = "SELECT * FROM c_p WHERE ID = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                c_p product = null;
                if (reader.Read())
                {
                    product = new c_p
                    {
                        ID = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        Price = (decimal)reader["Price"],
                        clickcounter = (int?)reader["clickcounter"] ?? 0
                    };
                }
                reader.Close();

                if (product == null)
                {
                    return RedirectToAction("ErrorPage");
                }

                // Increment clickcounter
                product.clickcounter += 1;

                // Update clickcounter in database
                string updateQuery = "UPDATE c_p SET clickcounter = @clickcounter WHERE ID = @id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@clickcounter", product.clickcounter);
                updateCmd.Parameters.AddWithValue("@id", product.ID);
                updateCmd.ExecuteNonQuery();

                // Insert into cart (assuming you have the user's ID)
                int userId = 1; // Replace with the actual user ID logic
                string insertQuery = "INSERT INTO cart (c_p_id, quantity, user_id) VALUES (@c_p_id, @quantity, @user_id)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@c_p_id", product.ID);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@user_id", userId);
                insertCmd.ExecuteNonQuery();
            }

            return RedirectToAction("cart");
        }
        private void IncreaseProductClickCounter(int productId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string updateQuery = "UPDATE c_p SET clickcounter = ISNULL(clickcounter, 0) + 1 WHERE ID = @productId";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@productId", productId);
                updateCmd.ExecuteNonQuery();
            }
        }


        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM cart WHERE cart_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult UpdateCart(int id, int quantity)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE cart SET quantity = @quantity WHERE cart_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            return Json(new { success = true });
        }

    }


}
