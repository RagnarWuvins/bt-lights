package bneumann.meisterlampe;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.Writer;
import java.util.ArrayList;
import java.util.Date;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import org.w3c.dom.Attr;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;
import org.xml.sax.Attributes;
import org.xml.sax.SAXException;
import org.xmlpull.v1.XmlSerializer;
import bneumann.meisterlampe.Lamp.Channel;
import android.os.Environment;
import android.util.Log;
import android.util.Xml;

public class XMLHandler
{
	private static final String TAG = "XMLHandler";
	private Lamp mLamp;
	private final String dataPath = "/MeisterLampe";
	private final String dataFile = "/UserSettings.xml";
	private File mUserSettings;
	private String[] mPropStrings = {"mode", "value", "delay", "period", "min", "max", "rise", "offset"};
	private XPath mXpath;
	private Document mXdoc;
	
	public XMLHandler(Lamp lamp) throws Exception
	{
		this.mLamp = lamp;
		File path = new File(String.format("%s%s",Environment.getExternalStorageDirectory(), dataPath));
		mUserSettings = new File(String.format("%s%s%s",Environment.getExternalStorageDirectory(), dataPath, dataFile));
		path.mkdirs();
		try
		{
			mUserSettings.createNewFile();
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
		DocumentBuilderFactory domFactory = DocumentBuilderFactory.newInstance();
		domFactory.setNamespaceAware(true);
		mXdoc = domFactory.newDocumentBuilder().parse(mUserSettings);
		mXpath = XPathFactory.newInstance().newXPath();
	}

	public ArrayList<String> GetHeader() throws Exception
	{
		ArrayList<String> returnList = new ArrayList<String>();
		// XPath Query for showing all nodes value
		String queryString = "//setting";
		NodeList nodes = (NodeList) xmlQuery(queryString);
		
		for (int i = 0; i < nodes.getLength(); i++)
		{
			Element e = (Element) nodes.item(i);
			returnList.add(e.getAttribute("CustomName"));
		}
		return returnList;
	}
	
	public boolean EditCurrentSetting(String name) throws Exception
	{
		
		String queryString = String.format("//setting[@CustomName='%s']",name);
		NodeList nodes = (NodeList) xmlQuery(queryString);
		// check if setting is already in the file
		if(nodes.getLength() == 0)
		{
			// quit with false because the setting doesn't exist
			return false;
		}
		//Get all channels
		queryString = String.format("//setting[@CustomName='%s']/channel",name);
		nodes = (NodeList) xmlQuery(queryString);
		
		for (int i = 0; i < nodes.getLength(); i++)
		{
			Element e = (Element) nodes.item(i);
			int channelNum = Integer.parseInt(e.getAttribute("number"));
			Channel curChan = mLamp.channels[channelNum];
			for(String propString : mPropStrings)
			{
				NodeList chanNode = e.getElementsByTagName(propString);
				String test = GetProperty(curChan, propString);
				chanNode.item(0).setTextContent(test);
			}
		}
		
		// Save the result
		Transformer xformer = TransformerFactory.newInstance().newTransformer();
		xformer.transform(new DOMSource(mXdoc), new StreamResult(mUserSettings));
		return true;
	}

	private Object xmlQuery(String queryString) throws XPathExpressionException
	{
		// XPath Query for showing all nodes value		
		XPathExpression expr = mXpath.compile(queryString);
		Object result = expr.evaluate(mXdoc, XPathConstants.NODESET);
		return result;
	}
	
	public void SaveData()
	{
		// In file nachsehen wie viele daten schon da sind und ID erhöhen!
		FileOutputStream fileos = null;
		try
		{
			fileos = new FileOutputStream(mUserSettings);
		}
		catch (FileNotFoundException e)
		{
			e.printStackTrace();
		}

		XmlSerializer serializer = Xml.newSerializer();
		try
		{
			serializer.setOutput(fileos, "UTF-8");
			serializer.startDocument(null, Boolean.valueOf(true));
			serializer.setFeature("http://xmlpull.org/v1/doc/features.html#indent-output", true);
			serializer.startTag(null, "root");
			serializer.startTag(null, "setting");
			serializer.attribute(null, "ID", "1");
			serializer.attribute(null, "CustomName", "UserSetting");
			//serializer.attribute(null, "Version", String.format("%d.%d", mLamp.HWVersion, mLamp.HWBuild));
			serializer.attribute(null, "Date", new Date().toString());
			
			for (int i = 0; i < Lamp.NUMBER_OF_CHANNELS; i++)
			{
				Channel curChan = mLamp.channels[i];

				serializer.startTag(null, "channel");				
				serializer.attribute(null, "number", String.valueOf(i));
				
				serializer.startTag(null, "mode");
				serializer.text(String.valueOf(curChan.getMode()));
				serializer.endTag(null, "mode");
				
				serializer.startTag(null, "value");
				serializer.text(String.valueOf(curChan.getValue()));
				serializer.endTag(null, "value");

				serializer.startTag(null, "delay");
				serializer.text(String.valueOf(curChan.getDelay()));
				serializer.endTag(null, "delay");
				
				serializer.startTag(null, "period");
				serializer.text(String.valueOf(curChan.getPeriod()));
				serializer.endTag(null, "period");
				
				serializer.startTag(null, "min");
				serializer.text(String.valueOf(curChan.getMin()));
				serializer.endTag(null, "min");				

				serializer.startTag(null, "max");
				serializer.text(String.valueOf(curChan.getMax()));
				serializer.endTag(null, "max");
				
				serializer.startTag(null, "rise");
				serializer.text(String.valueOf(curChan.getRise()));
				serializer.endTag(null, "rise");
				
				serializer.startTag(null, "offset");
				serializer.text(String.valueOf(curChan.getOffset()));
				serializer.endTag(null, "offset");
								
				serializer.endTag(null, "channel");
			}
			serializer.endTag(null, "setting");
			serializer.endTag(null, "root");			
			serializer.endDocument();
			serializer.flush();
			fileos.close();
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}
	}
	
	private String GetProperty(Channel channel, String propString)
	{
		// {"mode", "value", "delay", "period", "min", "max", "rise", "offset"
		String returnString = "0";

		if(propString == "mode")
		{
			returnString = String.valueOf(channel.getMode());
		}
		if (propString == "value")
		{
			returnString = String.valueOf(channel.getValue());
		}
		if (propString == "delay")
		{
			returnString = String.valueOf(channel.getDelay());
		}
		if (propString == "period")
		{
			returnString = String.valueOf(channel.getPeriod());
		}
		if (propString == "min")
		{
			returnString = String.valueOf(channel.getMin());
		}
		if (propString == "max")
		{
			returnString = String.valueOf(channel.getMax());
		}
		if (propString == "rise")
		{
			returnString = String.valueOf(channel.getRise());
		}
		if (propString == "offset")
		{
			returnString = String.valueOf(channel.getOffset());
		}

		return returnString;
	}
	
}
