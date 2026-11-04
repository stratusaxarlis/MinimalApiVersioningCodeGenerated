using MinimalApiVersioningCodeGenerated.Common.Attributes;
using MinimalApiVersioningCodeGenerated.Common.Enumerators;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.Interfaces;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;


// Features/Products/GetProductById.cs — v1 only (superseded in v2)
[Endpoint("{id}", Verb = HttpVerb.Get, UntilVersion = 1, Group = "Products")]
public sealed record GetProductById(Guid Id): IQuery<ProductResponse>;
[Endpoint("{id}", Verb = HttpVerb.Get, UntilVersion = 2, Group = "Products")]
public sealed record ProductResponse(Guid Id, string Name, decimal Price, string Currency );

// Features/Products/ListProducts.cs — stable across v1 and v2
[Endpoint("", Verb = HttpVerb.Get, UntilVersion = 2, Group = "Products")]
public sealed record ListProducts(int Page = 1, int Size = 20): IQuery<PagedResult<ProductResponse>>;

// Features/Products/CreateProduct.cs — v1 only (superseded in v2)
[Endpoint("", Verb = HttpVerb.Post,UntilVersion  = 1, Group = "Products")]
public sealed record CreateProduct(string Name, decimal Price, string Currency) : ICommand<Guid>, IProductData;
// ── v2 ────────────────────────────────────────────────────────

// Features/Products/V2/GetProductByIdV2.cs — v2 only (breaking response change)
[Endpoint("{id}", Verb = HttpVerb.Get, Version = 2, Group = "Products")]
public sealed record GetProductByIdV2(Guid Id): IQuery<ProductResponseV2>;
[Endpoint("{id}", Verb = HttpVerb.Get, Version = 2,UntilVersion = 2, Group = "Products")]
public sealed record ProductResponseV2(Guid Id, string Name, MoneyResponse Price);
[Endpoint("{id}", Verb = HttpVerb.Get, Version = 2, Group = "Products")]
public sealed record MoneyResponse(decimal Amount, string Currency);

// Features/Products/V2/CreateProductV2.cs — v2 only (new required field)
[Endpoint("", Verb = HttpVerb.Post, Version = 2, Group = "Products")]
public sealed record CreateProductV2(string Name, decimal Price, string Currency): ICommand<Guid>, IProductData;

// Features/Products/V2/DeleteProduct.cs — v2 only (new capability)
[Endpoint("{id}", Verb = HttpVerb.Delete, Version = 2, Group = "Products")]
public sealed record DeleteProduct(Guid Id): ICommand;