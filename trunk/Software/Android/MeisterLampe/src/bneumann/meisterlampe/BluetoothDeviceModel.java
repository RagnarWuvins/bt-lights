package bneumann.meisterlampe;

public class BluetoothDeviceModel
{
	private String Name = "";
	private String Address = "";
	private int IconId = R.drawable.ic_menu_more;
	
	public BluetoothDeviceModel()
	{
		
	}
	
	public BluetoothDeviceModel(String name, String address)
	{
		this.Name = name;
		this.Address = address;
	}
	
	public void setName(String name)
	{
		this.Name = name;
	}
	
	public void setAddress(String address)
	{
		this.Address = address;
	}
	
	public void setIcon(int iconid)
	{		
		this.IconId = iconid;
	}
	
	public String getName()
	{
		return this.Name;
	}
	
	public String getAddress()
	{
		return this.Address;
	}
	
	public int getIcon()
	{
		return this.IconId;
	}
	
}
