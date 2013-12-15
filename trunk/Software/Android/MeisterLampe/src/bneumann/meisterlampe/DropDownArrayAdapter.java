package bneumann.meisterlampe;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;
public class DropDownArrayAdapter extends ArrayAdapter<BluetoothDeviceModel>
{
	private Context mContext;

	public DropDownArrayAdapter(Context context)
	{
		super(context, R.layout.list);
		this.mContext = context;
	}

	public BluetoothDeviceModel[] getValues()
	{
		BluetoothDeviceModel[] values = new BluetoothDeviceModel[this.getCount()];
		for(int i = 0; i < values.length; i++)
		{
			values[i] = this.getItem(i);
		}
		return values;
	}
	
	@Override
	public View getDropDownView(int position, View convertView, ViewGroup parent)
	{	
		LayoutInflater inflater = (LayoutInflater)this.mContext.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
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
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{		
		return getDropDownView(position, convertView, parent);
	}
}
