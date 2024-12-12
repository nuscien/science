# Statistics

You may use `System.Math` class for most scenarios you will use mathematics.

In `Trivial.Maths` [namespace](../) of `Trivial.Maths.dll` [library](../../).

## Simple linear regression

Get the result of simple linear regression by initializing an instance of `SimpleLinearRegressionResult` value type with parameters intercept and slop. Then `GetX(double y)` and `GetY(double x)` methods can be used to get the point value.

## Gaussian mixture model

Get gaussian mixture models by calling `GaussianMixtureModelItem.Calculate(int count, double[] data, int maxIterations = 100, double tolerance = 1e-6, Random random = null)` function.

## Collection operations

Use functions in `CollectionOperations` static class to calculate number collections.
