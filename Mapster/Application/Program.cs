using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        // api yada mvc projesi gibi servis ekleme kısmı
        builder.Services.AddMapster();

        // projeyi tara ve IRegsiter ayarlamalarını bul
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly()); 

        IHost app = builder.Build();

        //hazırlanmış mapper sınıfını alma
        IMapper mapper = app.Services.GetRequiredService<IMapper>(); 

        UserEntity entity = new UserEntity() 
        { 
            Name = "Ahmet Taha", 
            Surname = "Mücasiroğlu", 
            Password = "123456",
            Birthdate = DateTime.Now 
        };
        UserDTO dto = mapper.Map<UserDTO>(entity); 
        Console.WriteLine(dto.ToString());

        /* Çıktı
            Name = Ahmet Taha
            Surname = Mücasiroğlu
            Birthdate = 16.08.2025 14:53:43
            IsBirthdate = True
        */

        Customer customer = new Customer() 
        { 
            Name = "Ahmet Taha", Surname = "Mücasiroğlu", Password = "123456" 
        };
        CustomerDTO customerDTO = mapper.Map<CustomerDTO>(customer);
        Console.WriteLine(customerDTO.ToString());
        /*Çıktı
            Name = Ahmet Taha
            Surname = Mücasiroğlu
         */

        await app.RunAsync();
    }
}

public class UserEntity
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }
}
public class UserDTO
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }
    public bool IsBirthdate { get; set; }

    public override string ToString()
    {
    return $"Name = {Name}" +
           $"\nSurname = {Surname}" +
           $"\nBirthdate = {Birthdate}" +
           $"\nIsBirthdate = {IsBirthdate}";
    }
}
public class UserMapProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserEntity, UserDTO>()
            .Map(dst => dst.IsBirthdate, src => src.Birthdate.Date == DateTime.Now.Date);
    }
}


public class Customer
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class CustomerDTO
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Name = {Name}\nSurname = {Surname}";
    }
}