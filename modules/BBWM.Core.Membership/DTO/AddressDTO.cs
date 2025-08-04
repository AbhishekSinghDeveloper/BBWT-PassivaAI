namespace BBWM.Core.Membership.DTO;

/// <summary>
/// Data transfer object for address
/// </summary>
public class AddressDTO
{
    /// <summary>
    /// Entity identity field
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the address1.
    /// </summary>
    /// <value>
    /// The address1.
    /// </value>
    public string Address1 { get; set; }

    /// <summary>
    /// Gets or sets the address2.
    /// </summary>
    /// <value>
    /// The address2.
    /// </value>
    public string Address2 { get; set; }

    /// <summary>
    /// Gets or sets the address3.
    /// </summary>
    /// <value>
    /// The address3.
    /// </value>
    public string Address3 { get; set; }

    /// <summary>
    /// Gets or sets the address4.
    /// </summary>
    /// <value>
    /// The address4.
    /// </value>
    public string Address4 { get; set; }

    /// <summary>
    /// Gets or sets the post code.
    /// </summary>
    /// <value>
    /// The post code.
    /// </value>
    public string PostCode { get; set; }

}
