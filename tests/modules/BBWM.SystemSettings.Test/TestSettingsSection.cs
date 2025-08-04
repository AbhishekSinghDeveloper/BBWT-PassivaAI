using System.Diagnostics.CodeAnalysis;

namespace BBWM.SystemSettings.Test;

public class TestSettingsSection : IEquatable<TestSettingsSection>
{
    public string TestProperty { get; set; }

    public bool Equals([AllowNull] TestSettingsSection other) => other.TestProperty.Equals(this.TestProperty);
}
