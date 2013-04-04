package bneumann.meisterlampe;

import java.util.ArrayList;
import bneumann.meisterlampe.Lamp.Channel;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.SeekBar;
import android.widget.Spinner;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.SeekBar.OnSeekBarChangeListener;

public class ChannelFragment extends Fragment
{

	private Switch channel_switch;
	private Spinner function_spinner;
	private SeekBar value_seekbar;
	private TextView value_text;
	public int channelNumber;
	public Lamp connectedLamp;
	public Channel channel;
	private TextView delay_text;
	private SeekBar delay_seekbar;
	private SeekBar period_seekbar;
	private TextView period_text;
	private SeekBar rise_seekbar;
	private TextView rise_text;

	public ChannelFragment()
	{
		this.connectedLamp = MainActivity.connectedLamp;
	}

	public static final String ARG_SECTION_NUMBER = "section_number";

	
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
	{

		Bundle args = getArguments();
		this.channelNumber = args.getInt("channelNumber");
		View curTab = inflater.inflate(R.layout.channel_view, container, false);
		// get UI elements
		channel_switch = (Switch) curTab.findViewById(R.id.channelEnable);
		function_spinner = (Spinner) curTab.findViewById(R.id.function_spinner);

		value_seekbar = (SeekBar) curTab.findViewById(R.id.value_seekbar);
		value_text = (TextView) curTab.findViewById(R.id.value_text);

		rise_seekbar = (SeekBar) curTab.findViewById(R.id.rise_seekbar);
		rise_text = (TextView) curTab.findViewById(R.id.rise_text);

		period_seekbar = (SeekBar) curTab.findViewById(R.id.period_seekbar);
		period_text = (TextView) curTab.findViewById(R.id.period_text);

		delay_text = (TextView) curTab.findViewById(R.id.delay_text);
		delay_seekbar = (SeekBar) curTab.findViewById(R.id.delay_seekbar);

		channel = connectedLamp.channels[channelNumber];

		SetupEnableControl();
		SetupRiseControl();
		SetupDelayControl();
		SetupPeriodControl();
		SetupValueControl();
		SetupFunctionControl(curTab);

		// Inflate the layout for this fragment
		return curTab;
	}

	public void OnResume()
	{
		super.onResume();
	}

	private void SetupEnableControl()
	{
		channel_switch.setChecked(!(channel.getMode() == Lamp.channelOff));
		channel_switch.setOnCheckedChangeListener(new OnCheckedChangeListener()
		{

			
			public void onCheckedChanged(CompoundButton buttonView, boolean isChecked)
			{
				if (isChecked)
				{
					channel.setMode((channel.lastMode == Lamp.channelOff) ? 0 : channel.lastMode);
				}
				else
				{
					channel.setMode(Lamp.channelOff);
				}

			}
		});
	}

	private void SetupFunctionControl(View curTab)
	{
		ArrayList<String> list = new ArrayList<String>();
		for (String listItem : CommandHandler.FUNCTION_NAMES)
		{
			list.add(listItem);
		}
		ArrayAdapter<String> adapter = new ArrayAdapter<String>(curTab.getContext(), android.R.layout.simple_list_item_1, list);
		adapter.notifyDataSetChanged();
		function_spinner.setAdapter(adapter);
		function_spinner.setSelection(channel.getMode());
		function_spinner.setOnItemSelectedListener(new OnItemSelectedListener()
		{

			
			public void onItemSelected(AdapterView<?> src, View view, int position, long id)
			{
				channel.setMode(position);
				channel_switch.setChecked(!(position == Lamp.channelOff));
			}

			
			public void onNothingSelected(AdapterView<?> arg0)
			{
				// TODO Auto-generated method stub

			}
		});
	}

	private void SetupValueControl()
	{
		value_seekbar.setMax(255);
		value_seekbar.setProgress(channel.getValue());
		value_seekbar.setOnSeekBarChangeListener(new OnSeekBarChangeListener()
		{

			
			public void onProgressChanged(SeekBar src, int progress, boolean fromUser)
			{
				channel.setValue(progress);
				value_text.setText(String.format("%d", channel.getValue()));
			}

			
			public void onStartTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}

			
			public void onStopTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}
		});
		value_text.setText(String.format("%d", channel.getValue()));
	}

	private void SetupRiseControl()
	{
		rise_seekbar.setMax(255);
		rise_seekbar.setProgress(channel.getRise());
		rise_seekbar.setOnSeekBarChangeListener(new OnSeekBarChangeListener()
		{

			
			public void onProgressChanged(SeekBar src, int progress, boolean fromUser)
			{
				channel.setRise(progress);
				rise_text.setText(String.format("%d", channel.getRise()));
			}

			
			public void onStartTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}

			
			public void onStopTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}
		});
		rise_text.setText(String.format("%d", channel.getRise()));
	}

	private void SetupDelayControl()
	{
		delay_seekbar.setMax(255);
		delay_seekbar.setProgress(channel.getDelay());
		delay_seekbar.setOnSeekBarChangeListener(new OnSeekBarChangeListener()
		{

			
			public void onProgressChanged(SeekBar src, int progress, boolean fromUser)
			{
				channel.setDelay(progress);
				delay_text.setText(String.format("%d", channel.getDelay()));
			}

			
			public void onStartTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}

			
			public void onStopTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}
		});
		delay_text.setText(String.format("%d", channel.getDelay()));
	}

	private void SetupPeriodControl()
	{
		period_seekbar.setMax(255);
		period_seekbar.setProgress(channel.getDelay());
		period_seekbar.setOnSeekBarChangeListener(new OnSeekBarChangeListener()
		{

			
			public void onProgressChanged(SeekBar src, int progress, boolean fromUser)
			{
				channel.setPeriod(progress);
				period_text.setText(String.format("%d", channel.getPeriod()));
			}

			
			public void onStartTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}

			
			public void onStopTrackingTouch(SeekBar src)
			{
				// TODO Auto-generated method stub

			}
		});
		period_text.setText(String.format("%d", channel.getPeriod()));
	}
}