package bneumann.meisterlampe;

import android.bluetooth.BluetoothAdapter;
import android.os.Build;

public class AppContext
{
	public boolean Emulator = Build.PRODUCT.contains("sdk");
	public BluetoothAdapter BTAdapter;
	
	public AppContext()
	{
		
	}
}
