package bneumann.meisterlampe;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class ListArrayAdapter extends ArrayAdapter<String>
{
	private Context mContext;

	public ListArrayAdapter(Context context)
	{		
		super(context, R.layout.list);
		this.mContext = context;
	}

	public String[] getValues()
	{
		String[] values = new String[this.getCount()];
		for(int i = 0; i < values.length; i++)
		{
			values[i] = this.getItem(i);
		}
		return values;
	}
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{
		LayoutInflater inflater = (LayoutInflater)this.mContext.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
		View rowView = inflater.inflate(R.layout.list, parent, false);
		
		String currentString = this.getValues()[position];
		String firstLine = "";
		String secondLine = "";		
		
		String[] splitString = currentString.split("\n");
		firstLine = splitString[0];
		if(splitString.length > 1)
		{
			secondLine = splitString[1];
		}
		
		TextView textViewFirst = (TextView) rowView.findViewById(R.id.ListFirstLine);
		textViewFirst.setText(firstLine);
		TextView textViewSecond = (TextView) rowView.findViewById(R.id.ListSecondLine);
		textViewSecond.setText(secondLine);

		return rowView;
	}
}
