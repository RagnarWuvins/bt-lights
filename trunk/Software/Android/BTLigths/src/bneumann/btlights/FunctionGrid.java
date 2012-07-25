package bneumann.btlights;

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.os.Bundle;
import android.view.Display;
import android.view.View;
import android.widget.Button;
import android.widget.TableLayout;
import android.widget.TableRow;

public class FunctionGrid extends Activity {
	/** Called when the activity is first created. */
	Command cmd;
	int channels;
	int functions;
	@Override	
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		TableLayout layout = new TableLayout(this);
		Display display = getWindowManager().getDefaultDisplay();
		int height = display.getHeight();
		int width = display.getWidth();

		layout.setLayoutParams(new TableLayout.LayoutParams(3, 10));

		layout.setPadding(1, 1, 1, 1);
		channels = 10;
		functions = 3;
		cmd = new Command();
		
		for (int ch = 0; ch <= channels; ch++) {
			TableRow tr = new TableRow(this);
			for (int func = 0; func < functions; func++) {
				final FunctionButton b = new FunctionButton(this);
				b.setText("CH" + ch + "|" + func);
				b.channel = ch;
				b.function = func;
				b.setTextSize(10.0f);
				b.setOnClickListener(new View.OnClickListener() {

					public void onClick(View v) {						
						byte[] command = cmd.assembleCommand(0, 4, b.channel, b.function);
						System.out.print("");
					}
				});
				tr.addView(b, width / functions, height / channels + 1);
			} // for
			layout.addView(tr);
		} // for
		super.setContentView(layout);
	}

	private class FunctionButton extends Button {
		public int function;
		public int channel;
		public FunctionButton(Context context) {
			super(context);
			this.function = 0;
			this.channel = 0;
		}

	}

}