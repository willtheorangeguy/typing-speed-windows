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
}
