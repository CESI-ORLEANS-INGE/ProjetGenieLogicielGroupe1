using System;
namespace EasySave.Model;

public interface ILog
{
    public Datetime Datetime { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string TypeDescription { get; set; }
    public double Filesize { get; set; }
    public double TransfertDuration { get; set; }


}
public class Log { }