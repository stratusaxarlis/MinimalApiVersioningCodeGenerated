using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.DTO;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.Persistence.TestRepository;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature;

/// <summary>
/// Composition for the Products feature slice: keeps feature-specific wiring out of Program.cs.
/// </summary>
public static class ProductsFeatureExtensions
{
	public static IServiceCollection AddProductsFeature(this IServiceCollection services)
	{
		services.AddSingleton<IRepository<Product>, InMemoryRepository<Product>>();
		return services;
	}

	public static WebApplication SeedProducts(this WebApplication app)
	{
		var repo = app.Services.GetRequiredService<IRepository<Product>>();
		repo.Seed(
			new Product(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Keyboard", 49.99m, "USD"),
			new Product(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Mouse", 24.99m, "EUR"),
			new Product(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Monitor", 199.00m, "USD")
		);

		return app;
	}
}
