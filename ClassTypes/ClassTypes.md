# Claude Code Tarafından yazıldı.

# .NET'te Modern Tip Sistemleri: File, Record ve Partial Sınıflar - Kapsamlı Rehber

## 🎯 Giriş

.NET ekosistemi, C# 9.0 ve sonraki sürümlerle birlikte yazılım geliştirme süreçlerini köklü bir şekilde değiştiren yeni tip sistemleri sundu. Bu yazıda, modern C# uygulamalarında kod organizasyonu, performans ve sürdürülebilirlik açısından kritik öneme sahip üç önemli özelliği derinlemesine inceleyeceğiz: **File-scoped types**, **Record types** ve **Partial classes**.

## 📚 İçindekiler

1. [Record Types - Değişmez Veri Modelleri](#record-types)
2. [File-scoped Types - Kapsülleme ve Güvenlik](#file-scoped-types)
3. [Partial Classes - Modüler Kod Organizasyonu](#partial-classes)
4. [Gerçek Dünya Senaryoları](#gerçek-dünya-senaryoları)
5. [Performans Karşılaştırmaları](#performans-karşılaştırmaları)
6. [En İyi Pratikler](#en-iyi-pratikler)

---

## Record Types

### Record Nedir?

Record'lar, C# 9.0 ile tanıtılan ve öncelikli olarak **değişmez (immutable) veri taşıyıcıları** olarak tasarlanmış referans tiplerdir. Otomatik olarak değer eşitliği semantiği, deconstruction desteği ve with-expression özelliklerini sağlarlar.

### Temel Sözdizimi

```csharp
// Basit record tanımı
public record Person(string FirstName, string LastName);

// Genişletilmiş record tanımı
public record Employee(string FirstName, string LastName, decimal Salary)
{
    public string FullName => $"{FirstName} {LastName}";
    
    // Validasyon ekleyebilirsiniz
    public Employee
    {
        if (Salary < 0)
            throw new ArgumentException("Maaş negatif olamaz");
    }
}

// Record struct (C# 10.0+)
public record struct Point(double X, double Y);

// Readonly record struct
public readonly record struct ImmutablePoint(double X, double Y);
```

### Record'ların Özellikleri

#### 1. **Değer Eşitliği (Value Equality)**

```csharp
var person1 = new Person("Ali", "Yılmaz");
var person2 = new Person("Ali", "Yılmaz");
var person3 = new Person("Ayşe", "Demir");

Console.WriteLine(person1 == person2); // True (değer eşitliği)
Console.WriteLine(person1 == person3); // False
Console.WriteLine(ReferenceEquals(person1, person2)); // False (farklı nesneler)
```

#### 2. **With Expressions - Non-destructive Mutation**

```csharp
var employee = new Employee("Mehmet", "Öz", 15000);
var promoted = employee with { Salary = 20000 };

Console.WriteLine($"Orijinal: {employee.Salary}"); // 15000
Console.WriteLine($"Terfi sonrası: {promoted.Salary}"); // 20000
```

#### 3. **Deconstruction**

```csharp
var person = new Person("Zeynep", "Kaya");

// Deconstruction kullanımı
var (firstName, lastName) = person;
Console.WriteLine($"Ad: {firstName}, Soyad: {lastName}");

// Pattern matching ile kullanım
if (person is Person("Zeynep", var soyad))
{
    Console.WriteLine($"Zeynep'in soyadı: {soyad}");
}
```

#### 4. **Otomatik ToString() Implementasyonu**

```csharp
var employee = new Employee("Can", "Demir", 18000);
Console.WriteLine(employee); 
// Çıktı: Employee { FirstName = Can, LastName = Demir, Salary = 18000 }
```

### Record Inheritance (Kalıtım)

```csharp
public abstract record Vehicle(string Brand, string Model);

public record Car(string Brand, string Model, int DoorCount) 
    : Vehicle(Brand, Model);

public record ElectricCar(string Brand, string Model, int DoorCount, int BatteryCapacity) 
    : Car(Brand, Model, DoorCount)
{
    public int Range => BatteryCapacity * 5; // Basit hesaplama
}

// Kullanım
var tesla = new ElectricCar("Tesla", "Model 3", 4, 75);
var copy = tesla with { BatteryCapacity = 100 };
```

### Positional Records vs Standard Records

```csharp
// Positional record - kısa ve öz
public record Product(string Name, decimal Price, int Stock);

// Standard record - daha fazla kontrol
public record DetailedProduct
{
    public string Name { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    
    private decimal _discount;
    public decimal Discount 
    { 
        get => _discount;
        init => _discount = value is >= 0 and <= 100 ? value : 0;
    }
    
    public decimal FinalPrice => Price * (1 - Discount / 100);
}
```

---

## File-scoped Types

### File-scoped Types Nedir?

C# 11 ile gelen `file` erişim belirleyicisi, bir tipin sadece tanımlandığı dosya içinde görünür olmasını sağlar. Bu özellik, implementation detaylarını gizlemek ve namespace kirliliğini önlemek için mükemmel bir araçtır.

### Temel Kullanım

```csharp
// UserService.cs dosyası

public class UserService
{
    private readonly IValidator<User> _validator = new UserValidator();
    
    public bool ValidateUser(User user)
    {
        return _validator.Validate(user);
    }
}

// Bu sınıf sadece UserService.cs içinde görünür
file class UserValidator : IValidator<User>
{
    public bool Validate(User user)
    {
        return !string.IsNullOrEmpty(user.Email) 
            && user.Email.Contains("@")
            && user.Age >= 18;
    }
}

file interface IValidator<T>
{
    bool Validate(T item);
}
```

### File-scoped Types Kullanım Senaryoları

#### 1. **Helper Sınıfları ve Utility Fonksiyonları**

```csharp
// OrderProcessor.cs

public class OrderProcessor
{
    public ProcessResult ProcessOrder(Order order)
    {
        var calculator = new PriceCalculator();
        var validator = new OrderValidator();
        
        if (!validator.IsValid(order))
            return ProcessResult.Failed("Geçersiz sipariş");
            
        var totalPrice = calculator.Calculate(order);
        
        // İşlem devam eder...
        return ProcessResult.Success(totalPrice);
    }
}

// Sadece bu dosyada kullanılan yardımcı sınıflar
file class PriceCalculator
{
    public decimal Calculate(Order order)
    {
        var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
        var tax = subtotal * 0.18m;
        var shipping = order.Items.Count > 5 ? 0 : 15.99m;
        
        return subtotal + tax + shipping;
    }
}

file class OrderValidator
{
    public bool IsValid(Order order)
    {
        return order != null 
            && order.Items?.Any() == true
            && order.CustomerId > 0;
    }
}

file record ProcessResult(bool IsSuccess, string Message, decimal? Total = null)
{
    public static ProcessResult Success(decimal total) 
        => new(true, "İşlem başarılı", total);
    
    public static ProcessResult Failed(string message) 
        => new(false, message);
}
```

#### 2. **Test Doubles ve Mock Implementasyonlar**

```csharp
// EmailService.cs

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class EmailService
{
    private readonly IEmailSender _sender;
    
    public EmailService(IEmailSender? sender = null)
    {
        // Production'da gerçek implementasyon inject edilir
        // Test veya development için file-scoped mock kullanılır
        _sender = sender ?? new MockEmailSender();
    }
    
    public async Task NotifyUserAsync(User user, string message)
    {
        await _sender.SendAsync(user.Email, "Bildirim", message);
    }
}

// Development/test için basit mock
file class MockEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        Console.WriteLine($"[MOCK EMAIL] To: {to}, Subject: {subject}");
        return Task.CompletedTask;
    }
}
```

#### 3. **Private Implementation Pattern**

```csharp
// Repository.cs

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> CreateAsync(T entity);
}

public class RepositoryFactory
{
    public static IRepository<T> Create<T>() where T : class
    {
        return new GenericRepository<T>();
    }
}

// Implementation detayları gizli
file class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<int, T> _store = new();
    private int _nextId = 1;
    
    public Task<T?> GetByIdAsync(int id)
    {
        _store.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }
    
    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(_store.Values.AsEnumerable());
    }
    
    public Task<int> CreateAsync(T entity)
    {
        var id = _nextId++;
        _store[id] = entity;
        return Task.FromResult(id);
    }
}
```

### File-scoped Types ile Namespace Organizasyonu

```csharp
// Analytics.cs

namespace MyApp.Analytics;

public class AnalyticsService
{
    private readonly List<IMetricCollector> _collectors = new()
    {
        new PerformanceCollector(),
        new UsageCollector(),
        new ErrorCollector()
    };
    
    public async Task<AnalyticsReport> GenerateReportAsync()
    {
        var metrics = new List<Metric>();
        
        foreach (var collector in _collectors)
        {
            metrics.AddRange(await collector.CollectAsync());
        }
        
        return new AnalyticsReport(metrics);
    }
}

// Tüm bu tipler sadece Analytics.cs içinde görünür
file interface IMetricCollector
{
    Task<IEnumerable<Metric>> CollectAsync();
}

file class PerformanceCollector : IMetricCollector
{
    public async Task<IEnumerable<Metric>> CollectAsync()
    {
        // CPU, Memory, Disk I/O metrikleri
        await Task.Delay(100); // Simülasyon
        return new[]
        {
            new Metric("CPU", 45.2),
            new Metric("Memory", 78.5),
            new Metric("Disk", 23.1)
        };
    }
}

file class UsageCollector : IMetricCollector
{
    public async Task<IEnumerable<Metric>> CollectAsync()
    {
        // Kullanım istatistikleri
        await Task.Delay(50);
        return new[]
        {
            new Metric("ActiveUsers", 1250),
            new Metric("RequestsPerSecond", 450)
        };
    }
}

file class ErrorCollector : IMetricCollector
{
    public async Task<IEnumerable<Metric>> CollectAsync()
    {
        // Hata metrikleri
        await Task.Delay(75);
        return new[]
        {
            new Metric("ErrorRate", 0.02),
            new Metric("FailedRequests", 12)
        };
    }
}

file record Metric(string Name, double Value);
```

---

## Partial Classes

### Partial Classes Nedir?

Partial sınıflar, bir sınıfın tanımının birden fazla dosyaya bölünmesine olanak tanır. Bu özellik özellikle kod üretimi (code generation), büyük sınıfların organizasyonu ve concern separation için kullanılır.

### Temel Kullanım

```csharp
// Customer.cs - Ana iş mantığı
public partial class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredDate { get; set; }
    
    public decimal CalculateDiscount()
    {
        var years = (DateTime.Now - RegisteredDate).TotalDays / 365;
        return years switch
        {
            < 1 => 0.05m,
            < 3 => 0.10m,
            < 5 => 0.15m,
            _ => 0.20m
        };
    }
}

// Customer.Validation.cs - Validasyon mantığı
public partial class Customer
{
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name)
            && !string.IsNullOrEmpty(Email)
            && Email.Contains("@")
            && RegisteredDate <= DateTime.Now;
    }
    
    public IEnumerable<string> GetValidationErrors()
    {
        if (string.IsNullOrEmpty(Name))
            yield return "İsim boş olamaz";
            
        if (string.IsNullOrEmpty(Email))
            yield return "Email boş olamaz";
        else if (!Email.Contains("@"))
            yield return "Geçersiz email formatı";
            
        if (RegisteredDate > DateTime.Now)
            yield return "Kayıt tarihi gelecekte olamaz";
    }
}

// Customer.DataAccess.cs - Veri erişim mantığı
public partial class Customer
{
    private static readonly string ConnectionString = "...";
    
    public async Task SaveAsync()
    {
        // Veritabanına kaydetme mantığı
        await Task.Delay(100); // Simülasyon
        Console.WriteLine($"Customer {Name} saved to database");
    }
    
    public static async Task<Customer?> LoadAsync(int id)
    {
        // Veritabanından yükleme mantığı
        await Task.Delay(100); // Simülasyon
        return new Customer 
        { 
            Id = id, 
            Name = "Sample Customer",
            Email = "customer@example.com",
            RegisteredDate = DateTime.Now.AddYears(-2)
        };
    }
}
```

### Partial Methods

C# 9.0 ve sonrası için genişletilmiş partial method özellikleri:

```csharp
// ProductService.cs - Ana servis
public partial class ProductService
{
    private readonly List<Product> _products = new();
    
    public void AddProduct(Product product)
    {
        // Partial method çağrısı - implementasyon başka dosyada
        OnProductAdding(product);
        
        if (!ValidateProduct(product))
            throw new InvalidOperationException("Geçersiz ürün");
            
        _products.Add(product);
        
        OnProductAdded(product);
        LogActivity($"Ürün eklendi: {product.Name}");
    }
    
    // Partial method tanımları
    partial void OnProductAdding(Product product);
    partial void OnProductAdded(Product product);
    public partial bool ValidateProduct(Product product);
    private partial void LogActivity(string message);
}

// ProductService.Events.cs - Event handling
public partial class ProductService
{
    public event EventHandler<Product>? ProductAdded;
    public event EventHandler<Product>? ProductValidated;
    
    partial void OnProductAdding(Product product)
    {
        Console.WriteLine($"Ürün ekleniyor: {product.Name}");
        // Ön işlemler, loglama, vb.
    }
    
    partial void OnProductAdded(Product product)
    {
        ProductAdded?.Invoke(this, product);
        // Bildirimler, cache temizleme, vb.
    }
}

// ProductService.Validation.cs - Validasyon
public partial class ProductService
{
    public partial bool ValidateProduct(Product product)
    {
        if (product == null)
            return false;
            
        if (string.IsNullOrWhiteSpace(product.Name))
            return false;
            
        if (product.Price <= 0)
            return false;
            
        if (product.Stock < 0)
            return false;
            
        ProductValidated?.Invoke(this, product);
        return true;
    }
}

// ProductService.Logging.cs - Loglama
public partial class ProductService
{
    private readonly List<string> _activityLog = new();
    
    private partial void LogActivity(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        _activityLog.Add(logEntry);
        Console.WriteLine(logEntry);
    }
    
    public IReadOnlyList<string> GetActivityLog() => _activityLog.AsReadOnly();
}
```

### Source Generators ile Partial Classes

```csharp
// Models/User.cs - Kullanıcı tanımlı kısım
[GenerateBuilder]
[GenerateValidator]
public partial class User
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public int Age { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}

// Generated/User.Builder.g.cs - Source generator tarafından üretilen
public partial class User
{
    public class Builder
    {
        private string? _username;
        private string? _email;
        private int _age;
        private DateTime _createdAt = DateTime.Now;
        
        public Builder WithUsername(string username)
        {
            _username = username;
            return this;
        }
        
        public Builder WithEmail(string email)
        {
            _email = email;
            return this;
        }
        
        public Builder WithAge(int age)
        {
            _age = age;
            return this;
        }
        
        public Builder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }
        
        public User Build()
        {
            if (string.IsNullOrEmpty(_username))
                throw new InvalidOperationException("Username is required");
                
            if (string.IsNullOrEmpty(_email))
                throw new InvalidOperationException("Email is required");
                
            return new User
            {
                Username = _username,
                Email = _email,
                Age = _age,
                CreatedAt = _createdAt
            };
        }
    }
}

// Generated/User.Validator.g.cs - Source generator tarafından üretilen
public partial class User
{
    public ValidationResult Validate()
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(Username))
            errors.Add(new ValidationError(nameof(Username), "Username boş olamaz"));
        else if (Username.Length < 3)
            errors.Add(new ValidationError(nameof(Username), "Username en az 3 karakter olmalı"));
            
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add(new ValidationError(nameof(Email), "Email boş olamaz"));
        else if (!Email.Contains("@"))
            errors.Add(new ValidationError(nameof(Email), "Geçersiz email formatı"));
            
        if (Age < 0 || Age > 150)
            errors.Add(new ValidationError(nameof(Age), "Yaş 0-150 arasında olmalı"));
            
        return new ValidationResult(errors);
    }
}
```

### Partial Classes ile Concern Separation

```csharp
// OrderManager.Core.cs - Temel iş mantığı
public partial class OrderManager
{
    private readonly Dictionary<int, Order> _orders = new();
    private int _nextOrderId = 1;
    
    public Order CreateOrder(Customer customer, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = _nextOrderId++,
            CustomerId = customer.Id,
            Items = items,
            CreatedDate = DateTime.Now,
            Status = OrderStatus.Pending
        };
        
        _orders[order.Id] = order;
        OnOrderCreated(order);
        
        return order;
    }
    
    public Order? GetOrder(int orderId)
    {
        _orders.TryGetValue(orderId, out var order);
        return order;
    }
}

// OrderManager.Processing.cs - Sipariş işleme
public partial class OrderManager
{
    public async Task<ProcessingResult> ProcessOrderAsync(int orderId)
    {
        var order = GetOrder(orderId);
        if (order == null)
            return ProcessingResult.Failed("Sipariş bulunamadı");
            
        try
        {
            // Stok kontrolü
            if (!await CheckInventoryAsync(order))
                return ProcessingResult.Failed("Yetersiz stok");
                
            // Ödeme işlemi
            if (!await ProcessPaymentAsync(order))
                return ProcessingResult.Failed("Ödeme başarısız");
                
            // Sipariş durumunu güncelle
            order.Status = OrderStatus.Processing;
            OnOrderProcessed(order);
            
            return ProcessingResult.Success(order.Id);
        }
        catch (Exception ex)
        {
            LogError($"Sipariş işleme hatası: {ex.Message}");
            return ProcessingResult.Failed($"İşlem hatası: {ex.Message}");
        }
    }
    
    private async Task<bool> CheckInventoryAsync(Order order)
    {
        await Task.Delay(100); // Simülasyon
        return order.Items.All(item => item.Quantity <= 100);
    }
    
    private async Task<bool> ProcessPaymentAsync(Order order)
    {
        await Task.Delay(200); // Simülasyon
        var total = order.Items.Sum(i => i.Price * i.Quantity);
        return total <= 50000; // Basit limit kontrolü
    }
}

// OrderManager.Events.cs - Event yönetimi
public partial class OrderManager
{
    public event EventHandler<Order>? OrderCreated;
    public event EventHandler<Order>? OrderProcessed;
    public event EventHandler<Order>? OrderCancelled;
    
    partial void OnOrderCreated(Order order)
    {
        OrderCreated?.Invoke(this, order);
        LogInfo($"Yeni sipariş oluşturuldu: {order.Id}");
    }
    
    partial void OnOrderProcessed(Order order)
    {
        OrderProcessed?.Invoke(this, order);
        LogInfo($"Sipariş işlendi: {order.Id}");
    }
    
    public void CancelOrder(int orderId)
    {
        var order = GetOrder(orderId);
        if (order != null && order.Status != OrderStatus.Cancelled)
        {
            order.Status = OrderStatus.Cancelled;
            OrderCancelled?.Invoke(this, order);
            LogInfo($"Sipariş iptal edildi: {orderId}");
        }
    }
}

// OrderManager.Logging.cs - Loglama
public partial class OrderManager
{
    private readonly List<LogEntry> _logs = new();
    
    private void LogInfo(string message)
    {
        _logs.Add(new LogEntry(LogLevel.Info, message));
    }
    
    private void LogError(string message)
    {
        _logs.Add(new LogEntry(LogLevel.Error, message));
    }
    
    public IReadOnlyList<LogEntry> GetLogs() => _logs.AsReadOnly();
    
    public record LogEntry(LogLevel Level, string Message, DateTime Timestamp = default)
    {
        public DateTime Timestamp { get; init; } = 
            Timestamp == default ? DateTime.Now : Timestamp;
    }
    
    public enum LogLevel { Info, Warning, Error }
}

// OrderManager.Statistics.cs - İstatistikler
public partial class OrderManager
{
    public OrderStatistics GetStatistics()
    {
        var orders = _orders.Values;
        
        return new OrderStatistics
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
            ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
            CompletedOrders = orders.Count(o => o.Status == OrderStatus.Completed),
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SelectMany(o => o.Items)
                .Sum(i => i.Price * i.Quantity),
            AverageOrderValue = orders.Any() 
                ? orders.Average(o => o.Items.Sum(i => i.Price * i.Quantity))
                : 0
        };
    }
    
    public record OrderStatistics
    {
        public int TotalOrders { get; init; }
        public int PendingOrders { get; init; }
        public int ProcessingOrders { get; init; }
        public int CompletedOrders { get; init; }
        public int CancelledOrders { get; init; }
        public decimal TotalRevenue { get; init; }
        public decimal AverageOrderValue { get; init; }
    }
}
```

---

## Gerçek Dünya Senaryoları

### Senaryo 1: E-Ticaret Sistemi

```csharp
// Domain/Product.cs
public record Product(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category
)
{
    public bool IsAvailable => Stock > 0;
    public decimal DiscountedPrice(decimal discountPercentage) =>
        Price * (1 - discountPercentage / 100);
}

// Services/ProductCatalog.cs
public partial class ProductCatalog
{
    private readonly List<Product> _products = new();
    private readonly ICacheService _cache;
    
    public ProductCatalog(ICacheService cache)
    {
        _cache = cache;
    }
    
    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        var cacheKey = $"search_{query}";
        
        // Cache kontrolü
        var cached = await _cache.GetAsync<IEnumerable<Product>>(cacheKey);
        if (cached != null)
            return cached;
            
        // Arama algoritması
        var results = SearchAlgorithm.Search(_products, query);
        
        // Cache'e kaydet
        await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5));
        
        return results;
    }
}

// File-scoped arama algoritması
file static class SearchAlgorithm
{
    public static IEnumerable<Product> Search(List<Product> products, string query)
    {
        var keywords = query.ToLower().Split(' ');
        
        return products
            .Where(p => keywords.All(k => 
                p.Name.ToLower().Contains(k) ||
                p.Description.ToLower().Contains(k) ||
                p.Category.ToLower().Contains(k)))
            .OrderByDescending(p => CalculateRelevance(p, keywords))
            .Take(20);
    }
    
    private static int CalculateRelevance(Product product, string[] keywords)
    {
        var score = 0;
        var text = $"{product.Name} {product.Description} {product.Category}".ToLower();
        
        foreach (var keyword in keywords)
        {
            if (product.Name.ToLower().Contains(keyword))
                score += 10;
            if (product.Description.ToLower().Contains(keyword))
                score += 5;
            if (product.Category.ToLower().Contains(keyword))
                score += 3;
        }
        
        return score;
    }
}

// Services/ProductCatalog.Analytics.cs
public partial class ProductCatalog
{
    private readonly List<SearchQuery> _searchHistory = new();
    
    partial void OnSearchPerformed(string query, int resultCount)
    {
        _searchHistory.Add(new SearchQuery(query, resultCount, DateTime.Now));
    }
    
    public IEnumerable<string> GetPopularSearches(int top = 10)
    {
        return _searchHistory
            .GroupBy(s => s.Query.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(top)
            .Select(g => g.Key);
    }
    
    file record SearchQuery(string Query, int ResultCount, DateTime Timestamp);
}
```

### Senaryo 2: Mikroservis API Gateway

```csharp
// Gateway/ApiGateway.cs
public partial class ApiGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly CircuitBreaker _circuitBreaker;
    
    public ApiGateway(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _circuitBreaker = new CircuitBreaker();
    }
    
    public async Task<ApiResponse<T>> CallServiceAsync<T>(
        string serviceName, 
        string endpoint, 
        HttpMethod method,
        object? payload = null)
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            var request = BuildRequest(serviceName, endpoint, method, payload);
            var response = await _httpClient.SendAsync(request);
            
            return await ProcessResponse<T>(response);
        });
    }
}

// Circuit Breaker implementasyonu
file class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private CircuitState _state = CircuitState.Closed;
    
    private const int FailureThreshold = 5;
    private const int TimeoutSeconds = 30;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.Now.Subtract(_lastFailureTime).TotalSeconds > TimeoutSeconds)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }
        
        try
        {
            var result = await action();
            
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _lastFailureTime = DateTime.Now;
            
            if (_failureCount >= FailureThreshold)
            {
                _state = CircuitState.Open;
            }
            
            throw;
        }
    }
    
    file enum CircuitState { Closed, Open, HalfOpen }
}

file class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException() 
        : base("Circuit breaker is open. Service is unavailable.") { }
}

// Gateway/ApiGateway.RateLimiting.cs
public partial class ApiGateway
{
    private readonly RateLimiter _rateLimiter = new();
    
    public async Task<bool> CheckRateLimitAsync(string clientId)
    {
        return await _rateLimiter.AllowRequestAsync(clientId);
    }
    
    file class RateLimiter
    {
        private readonly Dictionary<string, ClientRateInfo> _clients = new();
        private const int MaxRequestsPerMinute = 100;
        
        public Task<bool> AllowRequestAsync(string clientId)
        {
            lock (_clients)
            {
                if (!_clients.TryGetValue(clientId, out var info))
                {
                    info = new ClientRateInfo();
                    _clients[clientId] = info;
                }
                
                var now = DateTime.Now;
                if (now.Subtract(info.WindowStart).TotalMinutes >= 1)
                {
                    info.WindowStart = now;
                    info.RequestCount = 0;
                }
                
                info.RequestCount++;
                return Task.FromResult(info.RequestCount <= MaxRequestsPerMinute);
            }
        }
        
        file class ClientRateInfo
        {
            public DateTime WindowStart { get; set; } = DateTime.Now;
            public int RequestCount { get; set; }
        }
    }
}

// Gateway/ApiGateway.Caching.cs
public partial class ApiGateway
{
    private readonly ResponseCache _cache = new();
    
    public async Task<T?> GetCachedResponseAsync<T>(string key)
    {
        return await _cache.GetAsync<T>(key);
    }
    
    public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiry)
    {
        await _cache.SetAsync(key, value, expiry);
    }
    
    file class ResponseCache
    {
        private readonly Dictionary<string, CacheEntry> _cache = new();
        
        public Task<T?> GetAsync<T>(string key)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.ExpiryTime > DateTime.Now)
                    {
                        return Task.FromResult((T?)entry.Value);
                    }
                    
                    _cache.Remove(key);
                }
                
                return Task.FromResult(default(T));
            }
        }
        
        public Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            lock (_cache)
            {
                _cache[key] = new CacheEntry(value!, DateTime.Now.Add(expiry));
            }
            
            return Task.CompletedTask;
        }
        
        file record CacheEntry(object Value, DateTime ExpiryTime);
    }
}
```

---

## Performans Karşılaştırmaları

### Record vs Class Performans Testi

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class RecordVsClassBenchmark
{
    private readonly PersonClass _personClass = new("John", "Doe", 30);
    private readonly PersonRecord _personRecord = new("John", "Doe", 30);
    
    // Class tanımı
    public class PersonClass
    {
        public string FirstName { get; }
        public string LastName { get; }
        public int Age { get; }
        
        public PersonClass(string firstName, string lastName, int age)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is not PersonClass other) return false;
            return FirstName == other.FirstName && 
                   LastName == other.LastName && 
                   Age == other.Age;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, Age);
        }
    }
    
    // Record tanımı
    public record PersonRecord(string FirstName, string LastName, int Age);
    
    [Benchmark]
    public PersonClass CreateClass()
    {
        return new PersonClass("Jane", "Smith", 25);
    }
    
    [Benchmark]
    public PersonRecord CreateRecord()
    {
        return new PersonRecord("Jane", "Smith", 25);
    }
    
    [Benchmark]
    public bool CompareClass()
    {
        var person1 = new PersonClass("John", "Doe", 30);
        var person2 = new PersonClass("John", "Doe", 30);
        return person1.Equals(person2);
    }
    
    [Benchmark]
    public bool CompareRecord()
    {
        var person1 = new PersonRecord("John", "Doe", 30);
        var person2 = new PersonRecord("John", "Doe", 30);
        return person1 == person2;
    }
    
    [Benchmark]
    public PersonRecord WithExpression()
    {
        return _personRecord with { Age = 31 };
    }
    
    [Benchmark]
    public PersonClass CloneAndModifyClass()
    {
        return new PersonClass(_personClass.FirstName, _personClass.LastName, 31);
    }
}

// Benchmark Sonuçları (örnek):
// | Method                | Mean      | Error    | StdDev   | Allocated |
// |----------------------|-----------|----------|----------|-----------|
// | CreateClass          | 3.124 ns  | 0.041 ns | 0.036 ns | 40 B      |
// | CreateRecord         | 3.156 ns  | 0.039 ns | 0.034 ns | 40 B      |
// | CompareClass         | 8.234 ns  | 0.125 ns | 0.111 ns | 80 B      |
// | CompareRecord        | 7.892 ns  | 0.098 ns | 0.087 ns | 80 B      |
// | WithExpression       | 3.456 ns  | 0.052 ns | 0.046 ns | 40 B      |
// | CloneAndModifyClass  | 3.234 ns  | 0.044 ns | 0.039 ns | 40 B      |
```

### File-scoped Types Derleme Optimizasyonu

```csharp
// Derleme öncesi
public class Service
{
    private Helper _helper = new Helper();
    
    private class Helper
    {
        public void DoWork() { }
    }
}

// File-scoped ile
public class Service
{
    private Helper _helper = new Helper();
}

file class Helper
{
    public void DoWork() { }
}

// IL seviyesinde:
// - File-scoped types daha agresif inlining yapabilir
// - Namespace kirliliği önlenir
// - Assembly boyutu optimize edilir
// - JIT compiler daha iyi optimizasyon yapabilir
```

---

## En İyi Pratikler

### Record Types İçin En İyi Pratikler

#### ✅ Yapılması Gerekenler:

1. **Değişmez veri modelleri için kullanın**
```csharp
// İyi - DTO için record kullanımı
public record UserDto(int Id, string Name, string Email);

// İyi - Event için record kullanımı
public record OrderPlacedEvent(int OrderId, DateTime Timestamp, decimal Total);
```

2. **With expressions ile non-destructive updates yapın**
```csharp
var original = new Person("Ali", "Veli", 30);
var updated = original with { Age = 31 }; // Yeni instance oluşturur
```

3. **Value semantics gerektiğinde kullanın**
```csharp
public record Money(decimal Amount, string Currency)
{
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Para birimleri farklı");
        
        return this with { Amount = Amount + other.Amount };
    }
}
```

#### ❌ Kaçınılması Gerekenler:

1. **Mutable state için record kullanmayın**
```csharp
// Kötü - Mutable collection
public record BadExample(List<string> Items); // Items değiştirilebilir!

// İyi - Immutable collection
public record GoodExample(ImmutableList<string> Items);
```

2. **Entity veya aggregate root için kullanmayın**
```csharp
// Kötü - Domain entity olarak record
public record User(int Id, string Name) // Entity'ler genelde mutable olmalı

// İyi - Class kullanımı
public class User
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    
    public void ChangeName(string newName) { /* iş kuralları */ }
}
```

### File-scoped Types İçin En İyi Pratikler

#### ✅ Yapılması Gerekenler:

1. **Implementation detaylarını gizleyin**
```csharp
// İyi - Private helper'ları file-scoped yapın
public class PublicApi
{
    private readonly Validator _validator = new();
}

file class Validator { /* implementation */ }
```

2. **Test doubles için kullanın**
```csharp
// İyi - Test mock'ları için
file class MockDatabase : IDatabase
{
    public Task<T> GetAsync<T>(int id) => Task.FromResult(default(T)!);
}
```

#### ❌ Kaçınılması Gerekenler:

1. **Public API'lerde kullanmayın**
```csharp
// Kötü - Public tip file-scoped olamaz
file public class PublicService { } // Derleme hatası!
```

2. **Farklı dosyalar arası paylaşılan tipler için kullanmayın**
```csharp
// Kötü - Başka dosyadan erişilmesi gereken tip
file class SharedModel { } // Diğer dosyalardan erişilemez!
```

### Partial Classes İçin En İyi Pratikler

#### ✅ Yapılması Gerekenler:

1. **Mantıksal ayrım için dosya isimlendirmesi kullanın**
```csharp
// İyi - Açıklayıcı dosya isimleri
// Customer.cs - Ana mantık
// Customer.Validation.cs - Validasyon
// Customer.Events.cs - Event handling
// Customer.DataAccess.cs - Veri erişimi
```

2. **Code generation ile birlikte kullanın**
```csharp
// İyi - Generated kod için partial
public partial class Model { /* user code */ }

// Model.Generated.cs
public partial class Model { /* generated code */ }
```

#### ❌ Kaçınılması Gerekenler:

1. **Aşırı parçalamadan kaçının**
```csharp
// Kötü - Tek metodlu partial dosyalar
// Customer.GetName.cs - sadece GetName metodu
// Customer.SetName.cs - sadece SetName metodu
```

2. **Bağımlılıkları farklı dosyalara yaymayın**
```csharp
// Kötü - Constructor bir dosyada, kullanılan field başka dosyada
// File1.cs
public partial class Service
{
    public Service(ILogger logger) { _logger = logger; }
}

// File2.cs
public partial class Service
{
    private readonly ILogger _logger; // Karışıklık!
}
```

---

## Gelişmiş Kullanım Örnekleri

### Domain-Driven Design ile Record Kullanımı

```csharp
// Value Objects
public record EmailAddress
{
    public string Value { get; }
    
    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            throw new ArgumentException("Geçersiz email adresi");
            
        Value = value.ToLowerInvariant();
    }
    
    public static implicit operator string(EmailAddress email) => email.Value;
}

public record Money(decimal Amount, Currency Currency)
{
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Farklı para birimleri toplanamaz");
            
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    public static Money Zero(Currency currency) => new(0, currency);
}

public enum Currency { TRY, USD, EUR, GBP }

// Aggregate kullanımı
public class Order
{
    private readonly List<OrderLine> _lines = new();
    
    public int Id { get; private set; }
    public EmailAddress CustomerEmail { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public Money TotalAmount { get; private set; }
    
    public Order(int id, EmailAddress customerEmail)
    {
        Id = id;
        CustomerEmail = customerEmail;
        TotalAmount = Money.Zero(Currency.TRY);
    }
    
    public void AddLine(Product product, int quantity)
    {
        var line = new OrderLine(product.Id, product.Name, product.Price, quantity);
        _lines.Add(line);
        TotalAmount += line.LineTotal;
    }
}

// Immutable order line
public record OrderLine(
    int ProductId,
    string ProductName,
    Money UnitPrice,
    int Quantity
)
{
    public Money LineTotal => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);
}
```

### CQRS Pattern ile Record ve File-scoped Types

```csharp
// Commands/CreateOrderCommand.cs
public record CreateOrderCommand(
    int CustomerId,
    List<OrderItemDto> Items,
    ShippingAddress Address
) : ICommand<CreateOrderResult>;

public record OrderItemDto(int ProductId, int Quantity);
public record ShippingAddress(string Street, string City, string PostalCode);
public record CreateOrderResult(int OrderId, string OrderNumber, decimal Total);

// Handlers/CreateOrderHandler.cs
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _repository;
    private readonly OrderValidator _validator = new();
    
    public CreateOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<CreateOrderResult> HandleAsync(CreateOrderCommand command)
    {
        // Validasyon
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
            
        // İş mantığı
        var order = OrderFactory.Create(command);
        
        // Persistence
        await _repository.SaveAsync(order);
        
        return new CreateOrderResult(
            order.Id,
            order.OrderNumber,
            order.TotalAmount
        );
    }
}

// File-scoped validator
file class OrderValidator
{
    public ValidationResult Validate(CreateOrderCommand command)
    {
        var errors = new List<string>();
        
        if (command.CustomerId <= 0)
            errors.Add("Geçersiz müşteri ID");
            
        if (!command.Items.Any())
            errors.Add("Sipariş en az bir ürün içermelidir");
            
        if (command.Items.Any(i => i.Quantity <= 0))
            errors.Add("Ürün miktarı pozitif olmalıdır");
            
        return new ValidationResult(errors);
    }
}

file class OrderFactory
{
    private static int _orderCounter = 1000;
    
    public static Order Create(CreateOrderCommand command)
    {
        var orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{_orderCounter++:D6}";
        
        return new Order
        {
            Id = 0, // DB tarafından atanacak
            OrderNumber = orderNumber,
            CustomerId = command.CustomerId,
            Items = command.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList(),
            ShippingAddress = command.Address,
            CreatedAt = DateTime.Now,
            Status = OrderStatus.Pending
        };
    }
}

file record ValidationResult(List<string> Errors)
{
    public bool IsValid => !Errors.Any();
}
```

### Event Sourcing ile Record Types

```csharp
// Events tanımları
public abstract record DomainEvent(Guid AggregateId, DateTime OccurredAt);

public record AccountCreated(
    Guid AggregateId,
    string AccountNumber,
    string OwnerName,
    decimal InitialBalance,
    DateTime OccurredAt
) : DomainEvent(AggregateId, OccurredAt);

public record MoneyDeposited(
    Guid AggregateId,
    decimal Amount,
    string Description,
    DateTime OccurredAt
) : DomainEvent(AggregateId, OccurredAt);

public record MoneyWithdrawn(
    Guid AggregateId,
    decimal Amount,
    string Description,
    DateTime OccurredAt
) : DomainEvent(AggregateId, OccurredAt);

// Aggregate
public partial class BankAccount
{
    private readonly List<DomainEvent> _events = new();
    
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = "";
    public string OwnerName { get; private set; } = "";
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }
    
    public IReadOnlyList<DomainEvent> UncommittedEvents => _events.AsReadOnly();
    
    // Event sourcing - replay events
    public static BankAccount LoadFromHistory(IEnumerable<DomainEvent> history)
    {
        var account = new BankAccount();
        
        foreach (var @event in history)
        {
            account.Apply(@event);
        }
        
        return account;
    }
    
    private void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case AccountCreated created:
                ApplyAccountCreated(created);
                break;
            case MoneyDeposited deposited:
                ApplyMoneyDeposited(deposited);
                break;
            case MoneyWithdrawn withdrawn:
                ApplyMoneyWithdrawn(withdrawn);
                break;
        }
    }
}

// BankAccount.Commands.cs - Command handlers
public partial class BankAccount
{
    public static BankAccount Create(string accountNumber, string ownerName, decimal initialBalance)
    {
        if (initialBalance < 0)
            throw new InvalidOperationException("Başlangıç bakiyesi negatif olamaz");
            
        var account = new BankAccount();
        var @event = new AccountCreated(
            Guid.NewGuid(),
            accountNumber,
            ownerName,
            initialBalance,
            DateTime.Now
        );
        
        account.RaiseEvent(@event);
        return account;
    }
    
    public void Deposit(decimal amount, string description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Yatırılacak miktar pozitif olmalı");
            
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Hesap aktif değil");
            
        var @event = new MoneyDeposited(Id, amount, description, DateTime.Now);
        RaiseEvent(@event);
    }
    
    public void Withdraw(decimal amount, string description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Çekilecek miktar pozitif olmalı");
            
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Hesap aktif değil");
            
        if (Balance < amount)
            throw new InvalidOperationException("Yetersiz bakiye");
            
        var @event = new MoneyWithdrawn(Id, amount, description, DateTime.Now);
        RaiseEvent(@event);
    }
    
    private void RaiseEvent(DomainEvent @event)
    {
        _events.Add(@event);
        Apply(@event);
    }
}

// BankAccount.EventHandlers.cs - Event application logic
public partial class BankAccount
{
    private void ApplyAccountCreated(AccountCreated @event)
    {
        Id = @event.AggregateId;
        AccountNumber = @event.AccountNumber;
        OwnerName = @event.OwnerName;
        Balance = @event.InitialBalance;
        Status = AccountStatus.Active;
    }
    
    private void ApplyMoneyDeposited(MoneyDeposited @event)
    {
        Balance += @event.Amount;
    }
    
    private void ApplyMoneyWithdrawn(MoneyWithdrawn @event)
    {
        Balance -= @event.Amount;
    }
}

// File-scoped helper types
file enum AccountStatus
{
    Active,
    Suspended,
    Closed
}

// Event store implementation
file class InMemoryEventStore
{
    private readonly Dictionary<Guid, List<DomainEvent>> _events = new();
    
    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events)
    {
        if (!_events.ContainsKey(aggregateId))
            _events[aggregateId] = new List<DomainEvent>();
            
        _events[aggregateId].AddRange(events);
        await Task.CompletedTask;
    }
    
    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        if (_events.TryGetValue(aggregateId, out var events))
            return await Task.FromResult(events);
            
        return await Task.FromResult(Enumerable.Empty<DomainEvent>());
    }
}
```

---

## Sonuç

.NET'in modern tip sistemleri - **Record types**, **File-scoped types** ve **Partial classes** - yazılım geliştirme süreçlerinde önemli avantajlar sunar:

### 🎯 Record Types
- ✅ Değişmez veri modelleri için ideal
- ✅ Otomatik değer eşitliği ve with expressions
- ✅ DTO, Value Objects ve Event modelleme için mükemmel
- ✅ Daha az boilerplate kod

### 🔒 File-scoped Types
- ✅ Implementation detaylarını gizleme
- ✅ Namespace kirliliğini önleme
- ✅ Test ve mock implementasyonlar için ideal
- ✅ Daha iyi kapsülleme ve güvenlik

### 📦 Partial Classes
- ✅ Büyük sınıfları mantıksal parçalara ayırma
- ✅ Code generation ile uyumlu
- ✅ Concern separation
- ✅ Takım çalışması için ideal

Bu özellikleri doğru kullanarak:
- 🚀 Daha temiz ve sürdürülebilir kod yazabilir
- 🛡️ Daha güvenli ve kapsüllenmiş sistemler oluşturabilir
- 📈 Performansı optimize edebilir
- 👥 Takım verimliliğini artırabilirsiniz

Modern C# uygulamalarında bu tip sistemlerini kullanmak, kod kalitesini artırırken geliştirme sürecini de hızlandırır. Özellikle Domain-Driven Design, CQRS, Event Sourcing gibi modern mimari pattern'lerle birlikte kullanıldığında, bu özellikler gerçek değerlerini ortaya koyar.

---

## 🔗 Kaynaklar ve İleri Okuma

- [Microsoft Docs - Records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Microsoft Docs - File-scoped types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/file)
- [Microsoft Docs - Partial Classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods)
- [C# 9.0 Records Deep Dive](https://devblogs.microsoft.com/dotnet/c-9-0-on-the-record/)
- [C# 11 File-scoped Types](https://devblogs.microsoft.com/dotnet/csharp-11-preview-file-scoped-types/)

---

*Bu yazı, .NET ekosistemindeki modern tip sistemlerini kapsamlı bir şekilde ele almıştır. Sorularınız veya eklemek istedikleriniz varsa, lütfen yorumlarda paylaşın!*

