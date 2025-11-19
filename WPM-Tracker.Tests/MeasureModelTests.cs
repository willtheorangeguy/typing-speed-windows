namespace WPM_Tracker.Tests;

public class MeasureModelTests
{
    [Fact]
    public void MeasureModel_CanSetAndGetDateTimeTicks()
    {
        // Arrange
        var model = new MeasureModel();
        long expectedTicks = 638000000000000000;

        // Act
        model.DateTimeTicks = expectedTicks;

        // Assert
        Assert.Equal(expectedTicks, model.DateTimeTicks);
    }

    [Fact]
    public void MeasureModel_CanSetAndGetWpmValue()
    {
        // Arrange
        var model = new MeasureModel();
        double expectedWpm = 75.5;

        // Act
        model.WpmValue = expectedWpm;

        // Assert
        Assert.Equal(expectedWpm, model.WpmValue);
    }

    [Fact]
    public void MeasureModel_DefaultValues()
    {
        // Arrange & Act
        var model = new MeasureModel();

        // Assert
        Assert.Equal(0, model.DateTimeTicks);
        Assert.Equal(0, model.WpmValue);
    }

    [Fact]
    public void MeasureModel_CanSetMultipleProperties()
    {
        // Arrange
        var model = new MeasureModel();
        long expectedTicks = DateTime.Now.Ticks;
        double expectedWpm = 120.0;

        // Act
        model.DateTimeTicks = expectedTicks;
        model.WpmValue = expectedWpm;

        // Assert
        Assert.Equal(expectedTicks, model.DateTimeTicks);
        Assert.Equal(expectedWpm, model.WpmValue);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.5)]
    [InlineData(100.0)]
    [InlineData(150.75)]
    [InlineData(200.0)]
    public void MeasureModel_AcceptsVariousWpmValues(double wpmValue)
    {
        // Arrange
        var model = new MeasureModel();

        // Act
        model.WpmValue = wpmValue;

        // Assert
        Assert.Equal(wpmValue, model.WpmValue);
    }

    [Fact]
    public void MeasureModel_CanBeInstantiatedMultipleTimes()
    {
        // Arrange & Act
        var model1 = new MeasureModel { DateTimeTicks = 100, WpmValue = 50.0 };
        var model2 = new MeasureModel { DateTimeTicks = 200, WpmValue = 75.0 };

        // Assert
        Assert.Equal(100, model1.DateTimeTicks);
        Assert.Equal(50.0, model1.WpmValue);
        Assert.Equal(200, model2.DateTimeTicks);
        Assert.Equal(75.0, model2.WpmValue);
    }
}
