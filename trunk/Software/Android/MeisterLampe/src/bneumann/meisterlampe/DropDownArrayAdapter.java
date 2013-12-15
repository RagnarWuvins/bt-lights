package bneumann.meisterlampe;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;
public class DropDownArrayAdapter extends ListArrayAdapter
{

	public DropDownArrayAdapter(Context context)
	{
		super(context);
	}
	
	@Override
	public View getDropDownView(int position, View convertView, ViewGroup parent)
	{	
		return getView(position, convertView, parent);
	}
}
