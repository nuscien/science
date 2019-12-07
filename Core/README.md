﻿# [Trivial](https://github.com/nuscien/trivial/wiki/core)

This library includes utilities and services for tasks, IO, security, etc.

## [Tasks](https://github.com/nuscien/trivial/wiki/tasks)

Just add following namespace to your code file to use.

```csharp
using Trivial.Tasks;
```

### Hit task

You can create a task controller to manage when a handler should be raised.

- `HitTask.Debounce`:
  You may request to call a specific action several times in a short time but only the last one should be processed and previous ones should be ignored.
  A sample is real-time search suggestion.
- `HitTask.Throttle`:
  You may want to request to call an action only once in a short time even if you request to call several times.
  A sample is the submit button in a form.
- `HitTask.Times`:
  You can define an action can be only processed only when request to call in the specific times range and others will be ignored.
  A sample is double click.
- `HitTask.Multiple`:
  A handler to process for the specific times and the state will be reset after a while.

Following is an example for debounce.

```csharp
// An action you need to process.
Action action = () => {
    // Do something...
};

// Set up when the action can be processed.
var task = HitTask.Debounce(action, TimeSpan.FromMilliseconds(200));

// Somewhere to raise.
task.ProcessAsync();
```

### Retry

You can create a linear retry policy or even a customized to process an action with the specific retry policy.
And you can use `ObservableTask` to observe the state of an action processing.

## [Mathematics](https://github.com/nuscien/trivial/wiki/maths)

Just add following namespace to your code file to use.

```csharp
using Trivial.Maths;
```

### Arithmetic

There a lot of arithmetic functions.

```csharp
Arithmetic.IsPrime(2147483647); // True
await Arithmetic.IsPrimeAsync(2305843009213693951); // False

Arithmetic.Gcd(192, 128); // 64
Arithmetic.Lcm(192, 128); // 384
```

### Numerals

You can get the number symbols as you want.

You can also get the number string in English words.

```csharp
EnglishNumerals.Default.ToString(12345.67);
// twelve thousand three hundred and forty-five point six seven

EnglishNumerals.Default.ToApproximationString(1234567);
// 1.2M
```

And `ChineseNumerals` for Chinese and `JapaneseNumerals` for Japanese.

### Angle and polar point

- `Angle` Angle.
- `PolarPoint` The point in polar coordinates.
- `SphericalPoint` The point in spherical coordinates.

### Set

- `NullableValueSimpleInterval<T>` Interval, such as [20, 100).

### Rectangular coordinates

- `OneDimensionalPoint` The point in 1D (line) coordinates.
- `TwoDimensionalPoint` The point in 2D (flat) coordinates.
- `ThreeDimensionalPoint` The point in 3D (stereoscophic) coordinates.
- `FourDimensionalPoint` The point in 4D (spacetime) coordinates.

## [Network](https://github.com/nuscien/trivial/wiki/net)

Contains the helper functions and extension functions for network, such as HTTP web client and its content.

```csharp
using Trivial.Net;
```

And you can also use `JsonHttpClient` to serialize the JSON format response with retry policy supports.
And `HttpUri` for HTTP URI fields accessing.

## [Security](https://github.com/nuscien/trivial/wiki/security)

Just add following namespace to your code file to use.

```csharp
using Trivial.Security;
```

### RSA

You can convert a PEM (OpenSSL RSA key) or an XML string to the `RSAParametersConvert` class.

```csharp
var parameters = RSAParametersConvert.Parse(pem);
```

And you can convert back by using the extension method `ToPrivatePEMString` or `ToPublicPEMString`.
And also you can use the extension method `ToXElement` to export the XML.

### Symmetric

You can encrypt and decrypt a string by symmetric algorithm.

```csharp
// AES sample.
var original = "Original secret string";
var cipher = SymmetricUtilities.Encrypt(Aes.Create, original, key, iv);
var back = SymmetricUtilities.DecryptText(Aes.Create, cipher, key, iv); // back == original
```

### Hash

For hash algorithm, you can call `HashUtilities.ToHashString` function to get hash from a plain string and call `HashUtilities.Verify` to verify.

```csharp
var original = "The string to hash";

// SHA-512 (of SHA-2 family)
var sha512Str = HashUtilities.ToHashString(SHA512.Create, original);
var isVerified = HashUtilities.Verify(SHA512.Create, original, sha512Str); // --> true

// SHA-3-512
var sha3512Str = HashUtilities.ToHashString(new HashAlgorithmName("SHA3512"), original);
var isVerified3512 = HashUtilities.Verify(sha3512Name, original, sha3512Str); // --> true
```

### Access token

We also provide a set of tools for OAuth including following models.

- `TokenInfo` The access token and other properties.
- `AppAccessingKey` The app identifier and secret key.
- `OAuthClient` The token container with the ability to resolve the access token and create the JSON HTTP web client to access the resources required authentication.

And you can also implement the `OAuthBasedClient` base class to create your own business HTTP web client factory with OAuth supports.

### JWT

You can create a JSON web token to get the string encoded by initializing a new instance of the `JsonWebToken` class or the `JsonWebTokenParser` class.

```csharp
var sign = HashSignatureProvider.CreateHS512("a secret string");
var jwt = new JsonWebToken<Model>(new Model(), sign);
var jwtStr = jwt.ToEncodedString();

// Get authenticiation header value.
var header = jwt.ToAuthenticationHeaderValue();

// Parse.
var jwtSame = JsonWebToken<Model>.Parse(jwtStr, sign); // jwtSame.ToEncodedString() == jwtStr
```

### Secure string utiltiy

You can use the extension methods in the `SecureStringExtensions` class to convert the secret between `SecureString` and `String`/`StringBuilder`/`Byte[]`.

You can also use the class `RSASecretExchange` to transfer the secret with RSA encryption.

## [Data](https://github.com/nuscien/trivial/wiki/data)

Contains lots of data readers, parsers and utilities.

```csharp
using Trivial.Data;
```

### Data cache collection

You can create a collection to cache data with expiration and count limitation by following way.

```csharp
var cache = new DataCacheCollection<Model>
{
    MaxCount = 1000,
    Expiration = TimeSpan.FromSeconds(100)
};
```

So that you can get the data from the cache if has and or add new one if necessary.

```csharp
if (!cache.TryGet("abcd", out Model item)) item = new Model();
```

## [Text](https://github.com/nuscien/trivial/wiki/text)

Contains the text utilities.

```csharp
using Trivial.Text;
```

### CSV

You can read CSV file into a list of the specific models. For example, you have a model like following.

```csharp
class Model
{
    public string A { get; set; }
    public string B { get; set; }
}
```

Now you can map to the CSV file.

```csharp
var csv = new CsvParser("abcd,efg\nhijk,lmn");
foreach (var model in csv.ConvertTo<Model>(new[] { "A", "B" }))
{
    Console.WriteLine("{0},{1}", model.A, model.B);
}
```

### JSON

You can use `JsonJavaScriptTicksConverter` for JavaScript ticks serialization.

## Further

- [Geography](https://github.com/nuscien/trivial/wiki/geo)
- [Reflection](https://github.com/nuscien/trivial/wiki/reflection)
- [IO](https://github.com/nuscien/trivial/wiki/io)
