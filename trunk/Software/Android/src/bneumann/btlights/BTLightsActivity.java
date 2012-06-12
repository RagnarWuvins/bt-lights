package bneumann.btlights;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;

public class BTLightsActivity extends Activity {
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
    }
    
    public void sequencer_callback(View v)
    {
    	
    }
    
    public void about_callback(View v)
    {
    	Intent intent = new Intent(BTLightsActivity.this, AboutBTLightsActivity.class);
    	startActivity(intent); 
    }
}