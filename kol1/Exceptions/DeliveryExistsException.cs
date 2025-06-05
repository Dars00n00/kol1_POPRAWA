namespace kol1.Exceptions;

public class DeliveryExistsException(string message) : Exception(message);

public class CustomerNotFoundException(string message) : Exception(message);

public class DriverNotFoundException(string message) : Exception(message);

public class ProductNotFoundException(string message) : Exception(message);