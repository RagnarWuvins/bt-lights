package bneumann.meisterlampe;

import android.bluetooth.BluetoothDevice;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

public class ListArrayAdapter extends ArrayAdapter<BluetoothDeviceModel>
{
	private Context mContext;

	public ListArrayAdapter(Context context)
	{
		super(context, R.layout.list);
		this.mContext = context;
	}

	public boolean contains(BluetoothDeviceModel bdm)
	{
		for (int i = 0; i < this.getCount(); i++)
		{
			if (this.getItem(i).getAddress().equals(bdm.getAddress()) && this.getItem(i).getName().equals(bdm.getName()))
			{
				return true;
			}
		}
		return false;
	}
	

	public int getPositionByString(String name)
	{
		for (int i = 0; i < this.getCount(); i++)
		{
			if (this.getItem(i).getName().equals(name))
			{
				return i;
			}
		}
		return -1;
	}
	
	public boolean contains(BluetoothDevice bd)
	{
		return this.contains(new BluetoothDeviceModel(bd.getName(), bd.getAddress()));
	}

	public BluetoothDeviceModel[] getValues()
	{
		BluetoothDeviceModel[] values = new BluetoothDeviceModel[this.getCount()];
		for (int i = 0; i < values.length; i++)
		{
			values[i] = this.getItem(i);
		}
		return values;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{
		LayoutInflater inflater = (LayoutInflater) this.mContext.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
		View rowView = inflater.inflate(R.layout.list, parent, false);

		BluetoothDeviceModel currentDevice = this.getValues()[position];

		TextView textViewFirst = (TextView) rowView.findViewById(R.id.ListFirstLine);
		textViewFirst.setText(currentDevice.getName());
		TextView textViewSecond = (TextView) rowView.findViewById(R.id.ListSecondLine);
		textViewSecond.setText(currentDevice.getAddress());
		ImageView imageView = (ImageView) rowView.findViewById(R.id.ListIcon);
		imageView.setImageResource(currentDevice.getIcon());

		return rowView;
	}
}
