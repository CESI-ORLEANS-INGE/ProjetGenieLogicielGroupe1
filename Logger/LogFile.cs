using System;
using ILog;
namespace EasySave.Model;

public interface ILogFile
{
    public void Save(ILog Log) { get; }
    public List <ILog> Read() { get; }


}
public class LogFile { }