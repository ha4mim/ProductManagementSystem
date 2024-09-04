$(document).ready(function () {
    $('#customerSelect').change(function () {
        var customerId = $(this).val();
        if (customerId) {
            $.getJSON('/Customers/GetCustomerDetails', { id: customerId }, function (customer) {
                $('#customerDetails').html(`
                    <p>Name: ${customer.Name}</p>
                    <p>Address: ${customer.Address}</p>
                    <p>Phone: ${customer.Phone}</p>
                    <p>Email: ${customer.Email}</p>
                `);
            });
        }
    });

    $('#quantity').on('input', function () {
        var quantity = $(this).val();
        var pricePerUnit = '@Model.Product.PricePerUnit';
        var totalAmount = quantity * pricePerUnit;
        $('#totalAmount').text(totalAmount.toFixed(2));
    });

    $('#addNewCustomerBtn').click(function () {
        // Redirect to Add New Customer page or show a modal for adding a new customer
        window.location.href = '/Customers/Create';
    });
});
