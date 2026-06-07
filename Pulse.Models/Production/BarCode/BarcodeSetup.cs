using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pulse.Models.Production.BarCode;

public class BarcodeSetup
{
    [Key]
    public int BarcodeSetupID { get; set; }
    [DefaultValue(0)]
    public BarcodeCategory Category { get; set; } = 0;
    [Required]
    [DefaultValue(0)]
    public int VersionNo { get; set; } = 0;
    [DefaultValue(true)]
    public bool IsQRCode { get; set; } = true;
    [DefaultValue(0)]
    public int? BarcodeType { get; set; } = 0;
    [MaxLength(9)]
    [DefaultValue("#000000")]
    [Required]
    public required string Foreground { get; set; } = "#000000";
    [MaxLength(9)]
    [DefaultValue("#FFFFFF")]
    [Required]
    public required string Background { get; set; } = "#FFFFFF";
    [DefaultValue(true)]
    public bool bcShowValue { get; set; } = true;
    [DefaultValue(true)]
    public bool bcShowChecksum { get; set; } = true;
    [DefaultValue(12)]
    public int bcFontSize { get; set; } = 12;
    [DefaultValue(600)]
    public int bcFontWeight { get; set; } = 600;
    [MaxLength(9)]
    [DefaultValue("#000000")]
    [Required]
    public required string qrEyeColour { get; set; } = "#000000";
    [DefaultValue(0)]
    public int qrEyeShape { get; set; } = 0;
    [DefaultValue(0)]
    public int qRModuleShape { get; set; } = 0;
    public string? qrImage { get; set; }
    [MaxLength(9)]
    [DefaultValue("#FFFFFF")]
    [Required]
    public required string qrImageBackground { get; set; } = "#FFFFFF";
    [Required]
    public string? codeValue { get; set; }
    [DefaultValue(false)]
    public bool IsActive { get; set; } = true;
}
public enum BarcodeCategory
{
    EquipmentItem,
    ClientRoller,
    EmployeeID
}
