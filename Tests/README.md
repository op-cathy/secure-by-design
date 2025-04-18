# Workshop Guide: Testing

- [Workshop Guide: Testing](#workshop-guide-testing)
- [Introduction](#introduction)
  - [Security Requirements](#security-requirements)
- [Part 1 - Unit Tests](#part-1---unit-tests)
  - [Step 0](#step-0)
  - [Step 1 - ProductId Tests](#step-1---productid-tests)
  - [Step 2 - ProductService Tests](#step-2---productservice-tests)
  - [Step 3 - ProductController Tests](#step-3---productcontroller-tests)
- [Part 2 - Integration Tests](#part-2---integration-tests)
  - [Step 0 - Add client-secrets to Test Config](#step-0---add-client-secrets-to-test-config)
  - [Step 1 - BaseTests class and Authenticating with the API](#step-1---basetests-class-and-authenticating-with-the-api)
  - [Step 2 - Product Tests](#step-2---product-tests)
- [Part 3 (optional) - Create more tests](#part-3-optional---create-more-tests)

# Introduction

This part of the course will guide you through how you can write tests for the web API created earlier in the workshop. These tests will ensure your application follows defence-in-depth design principles. 
In this test project we will implement unit tests and integration tests. These are located in the Unit and System folders respectively.

The unit tests, located in the `./Unit`-folder will test the logic of individual units/methods within the application. In order to properly test only one unit at a time, dependencies should be mocked. Put simply: When we are testing a method, we only want to test the code within that method. Therefore, if that method relies on other parts of our codebase, we will replace those dependencies with dummies using a mocking library.

The integration tests, located in the `./System`-folder will test how units work with each other. In this workshop, we will focus these tests around the security in the API controllers and their integration with the underlying data services. More specifically, we can test this by sending various requests to the API and checking what data is available to us with different levels of authentication.

## Security Requirements

If we think of security vulnerabilities as bugs, then security is just another non-functional requirement of our application and should be part of quality assurance (QA). The IT industry has well-established patterns and practices for QA, where one is Test Driven Development (TDD).

To illustrate the need for explicitly defined security requirements and proper testing of these requirements within our application, lets consider the following scenario:

    We have a system with three APIs and three types of clients that need to access one or more of these APIs

<img src="../Resources/test_api_requirements.png" alt="Example architecture with three APIs" width=600/>

To define all user functionality in one requirement we could state the following:
- Users should be able to consume the APIs they need to provide the required user functionality.

This would meet all functional requirements but from a security point of view, it does not ensure proper access control and violates the principle of least privilege because some roles are able to access resources only intended for other roles.

To address this issue, we could specify our requirement as:
- Users should _only_ be able to consume the APIs they need to provide the required user functionality.

This sounds easy and completely obvious, but teams often fail to identify the test cases needed to avoid vulnerabilities. So how do we verify that users are ___only___ able to access the resource they absolutely need? A common practice is to add negative test cases such as:
- Regular users should _not_ be able to access API3.

This ensures that regular users are only able to access API1 and API2. Similar test cases should be applied for the other user types. 

# Part 1 - Unit Tests

First we will create unit tests for the application, ensuring that each individual component of our Web API works as intended.

## Step 0

The test project is located in the `Tests`-folder and is named `Tests.0-starting-point`. You can start by running the tests in this project by executing `dotnet test` in the CLI from within the `Tests/0-starting-point/`. A fully implemented solution is available in `Tests/completed/` if you are stuck.

For each of the steps below, empty test methods are provided. To better describe what is being tested in each method,the test methods are named using the following naming pattern:

    public void NameOfTheMethodWeAreTesting_WhatShouldHappen_Scenario()

For example:  

    public async Task GetById_ShouldReturn404_WhenNotFound()

## Step 1 - ProductId Tests

Our first set of unit tests will test the ProductId domain primitive, which is responsible for validating that all product IDs are valid. To test that this works as intended, we will test that this class allows creation of valid product IDs and that an exception is thrown for invalid IDs.

Implement the three test methods defined in `ProductIdTests.cs`

<details>
<summary><b>Hints (Test methods 1 and 2)</b></summary>
<p>

- The first test method (`Constructor_Should_Reject_InvalidData`) runs once for each line in the file `blns-injection.txt`.

- What do you expect the constructor in the `ProductId`-class to do when it is given an invalid product ID?

- In Xunit, you can verify that a method throws a specific exception using `Assert.Throws<ExceptionType>(() => MethodToTest())`.

</p>
</details>

<details>
<summary><b>Hints (Test method 3)</b></summary>
<p>

- The third test method runs once for each input defined by `InlineData`.

- After creating a new ProductId, what do you expect the value of that productId to be?

- In Xunit, you can verify that two values are equal using `Assert.Equal(actualValue, expectedValue)`.

</p>
</details>

[__Spoiler (full code)__](./completed/Unit/ProductIdTests.cs)


## Step 2 - ProductService Tests

Moving upwards in terms of complexity, the next set of tests will focus on the ProductService class. As this class uses both the ProductRepository and the PermissionService classes, we will need to mock these dependencies.

This is easily achieved using a third-party mocking library such as NSubstitute or FakeItEasy. In this workshop we will be using NSubstitute. When using NSubstitute, we can create 'substitutes' (i.e. 'mocks') of the ProductRepository and the PermissionService by using their respective interfaces:

```csharp
var productRepository = Substitute.For<IProductRepository>();
var permissionService = Substitute.For<IPermissionService>();
```

These mocks can then be used to create an instance of ProductService like so:

```csharp
var productService = new ProductService(productRepository, permissionService);
```

To specify return values for properties or method calls for the mocked dependencies, we can do so by utilising the `Returns`-extension method accessible through the substitute:

For properties and expression-bodied members we can use .Returns() directly:
```csharp
// permissionService.CanReadProducts should return true
permissionService
    .CanReadProducts
    .Returns(true);
```

For methods we have to specify the nature of the argument we are expecting:
```csharp
// permissionService.HasPermissionToMarket() should return false when the
// argument given is a MarketId with an arbitrary value
permissionService
    .HasPermissionToMarket(Arg.Any<MarketId>())
    .Returns(false);

// productRepository.GetBy(productId) should return a dummy product when the
// argument given is a ProductId with a specific value
var productId = new ProductId("123abc");
productRepository
    .GetBy(Arg.Is(productId))
    .Returns(
        new Product(
            productId,
            new ProductName("Product 1"),
            new Money(9m, "USD"),
            new MarketId("no")
        )
    );
```

Complete the test methods defined in `ProductServiceTests.cs`.

<details>
<summary><b>Hints (Test method 1)</b></summary>
<p>

- The first test method should run the `GetWith`-method on an instance of `ProductService`:

- The return value from running this method should be a `(ReadDataResult, Product?)`-tuple.

- The `ReadDataResult` should be `NoAccessToOperation` and the `Product` should be `null`.

- You can verify a value is null with Xunit using `Assert.Null(actualValue)`.

</p>
</details>

<details>
<summary><b>Hints (Test method 2)</b></summary>
<p>

- How do we set up the scenario `IfValidClaimButNotExisting`? (Meaning the user has a valid claim, but the requested product does not exist)

- The mocked `PermissionService` should return true for the property `CanReadProducts`.

- The `ReadDataResult` should be `NotFound` and the `Product` should be `null`.

- You can verify a value is null with Xunit using `Assert.Null(actualValue)`.

</p>
</details>

<details>
<summary><b>Hints (Test method 3)</b></summary>
<p>

- How do we set up the scenario `IfNotValidMarket`? (Meaning the user has a valid claim, but the user has no access to the requested market)

- The mocked `PermissionService` should return true for the property `CanReadProducts`.

- The mocked `PermissionService` should return false for calls to the method `HasPermissionToMarket`.

- The `ReadDataResult` should be `NoAccessToData` and the `Product` should be `null`.

</p>
</details>

<details>
<summary><b>Hints (Test method 4)</b></summary>
<p>

- How do we set up the scenario `IfValidClaim`? (Meaning the user has a valid claim and the user has access to the requested market)

- The mocked `PermissionService` should return true for the property `CanReadProducts`.

- The mocked `PermissionService` should return true for calls to the method `HasPermissionToMarket`.

- The `ReadDataResult` should be `Success` and the `Product` should not be `null`.

</p>
</details>

[__Spoiler (full code)__](./completed/Unit/ProductServiceTests.cs)


## Step 3 - ProductController Tests

Finally we will test the methods in the ProductController class. Again, since these are unit tests, we will need to mock all dependencies to the ProductController class. In this case we will just need to create a mock that implements the `IProductService` interface, and define some return value for the methods we are using in that interface.

<details>
<summary><b>Hints (Test method 1)</b></summary>
<p>

- This test method should call the controller and verify that it returns a 200 OK when the `ProductService` is properly mocked.

- How do we set up the mocked `ProductService` scenario `WhenAuthorized`? (Meaning the `ProductService` should return a valid object when called)

- The mocked `PermissionService` should return a `ReadDataResult.Success` and valid `Product` when called by the controller.

- The result from calling the controller should be an `OkObjectResult`.

- In Xunit, you can verify that a value is of a specific type with `Assert.IsType<ExpectedType>(value)`

</p>
</details>

<details>
<summary><b>Hints (Test method 2)</b></summary>
<p>

- This test method should call the controller and verify the returned object is an instance of `ProductDTO`.

- The scenario in this test method is the same as in the first method.

</p>
</details>

<details>
<summary><b>Hints (Test method 3)</b></summary>
<p>

- This test method should call the controller and verify that it returns a 400 BadRequest when the `ProductService` is properly mocked.

- What do you expect the controller to do when it receives an invalid `ProductId`?

- The result type from calling the controller should be a `BadRequestObjectResult`.

- The response body/value should be null.

</p>
</details>

<details>
<summary><b>Hints (Test method 4)</b></summary>
<p>

- This test method should call the controller and verify that it returns a 404 NotFound when the `ProductService` is properly mocked.

- What should the mocked `ProductService` return when a productId is not found?

- The result type from calling the controller should be a `NotFoundResult`.

- The response body/value should be null.

</p>
</details>

<details>
<summary><b>Hints (Test method 5)</b></summary>
<p>

- This test method should call the controller and verify that it returns a 403 Forbidden when the `ProductService` is properly mocked.

- What should the mocked `ProductService` return when a user does not have right claim for read access?

- The result type from calling the controller should be a `ForbidResult`.

- The response body/value should be null.

</p>
</details>

<details>
<summary><b>Hints (Test method 6)</b></summary>
<p>

- This test method should call the controller and verify that it returns a 404 NotFound when the `ProductService` is properly mocked.

- What should the mocked `ProductService` return when the user does not have access to the requested product?

- The result type from calling the controller should be a `NotFoundResult`.

- The response body/value should be null.

</p>
</details>

[__Spoiler (full code)__](./completed/Unit/ProductsControllerTests.cs)

# Part 2 - Integration Tests

To check that the inidividual parts of our system is working correctly when interacting with each other, we will create some integration tests in addition to the unit tests. Here we will not be mocking dependencies, and instead be testing the system as a whole.

The integration tests are located in `Tests/0-starting-point/System/`.

## Step 0 - Add client-secrets to Test Config

It is important to add the client secrets to `Tests/0-starting-point/testsettings.json`, otherwise the integration tests will not work.

The secrets can be obtained by asking your workshop instructor.

## Step 1 - BaseTests class and Authenticating with the API

To start off, have a look at `BaseTests.cs`, where we have defined SetUp and TearDown methods for the tests, as well as a method for authorizing the HttpClient we will use with the API we are testing.

To send requests to the SalesAPI, simply use the `_client` when inheriting from `BaseTests.cs`:

```csharp
var response = await _client.GetAsync("api/product/123GQWE");
```

and to authenticate with the API, call the `AuthorizeHttpClient`-method first with the desired scope.

```csharp
await AuthorizeHttpClient(ProductScope.Read);
```

## Step 2 - Product Tests

The `ProductTests` class in `ProductTests.cs` inherits from the `BaseTests` class which lets us easily use the HttpClient and authorize it for the tests that need authorization. 

Write tests for the product API testing the `/api/product/{id}` endpoint to receive proper status codes given the different authorizations.

<details>
<summary><b>Hints (Test method 1)</b></summary>
<p>

- This test method should verify that when anonymous users (i.e. unauthorized users) call the API, they should receive a 401 Unauthorized response code.

- To call the API, use the `_client` property from the base class: `await _client.GetAsync("api/product/<productId>")`

- Did you remember to add the client secrets to the `testsettings.json`-file in the `0-starting-point` folder?

</p>
</details>

<details>
<summary><b>Hints (Test method 2)</b></summary>
<p>

- This test method should verify that when authorized users with the wrong scope call the API, they should receive a 403 Forbidden response code.

- To authorize requests sent using `_client.GetAsync`, first do a call `await AuthorizeHttpClient(<scope>)` with the desired scope.

- Running GET requests to fetch products from the API requires

</p>
</details>

<details>
<summary><b>Hints (Test method 3)</b></summary>
<p>

- This test method should verify that when authorized users with correct scope call the API, they should receive a 200 OK response code.

- To authorize requests sent using `_client.GetAsync`, first do a call `await AuthorizeHttpClient(<scope>)` with the desired scope.

</p>
</details>

[__Spoiler (full code)__](./completed/System/ProductTests.cs)

# Part 3 (optional) - Create more tests

Congratulations! You have now finished the main part of this testing workshop. Because different parts of this application contained similar logic, not all parts were included in this testing workshop. Feel free to continue creating more tests for the remaining parts of the application. However, these tests will be quite similar to the ones you have already created.
