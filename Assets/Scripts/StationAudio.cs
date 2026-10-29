using UnityEngine;
namespace NorthernLab {
public class StationAudio : MonoBehaviour {
 static AudioSource effects; AudioSource wind, hum; float knock;
 void Start(){effects=gameObject.AddComponent<AudioSource>();wind=gameObject.AddComponent<AudioSource>();hum=gameObject.AddComponent<AudioSource>();wind.clip=Resources.Load<AudioClip>("wind");wind.loop=true;wind.volume=.22f;wind.Play();hum.clip=Resources.Load<AudioClip>("hum");hum.loop=true;hum.volume=.1f;hum.Play();}
 void Update(){var g=FindFirstObjectByType<StationGame>();if(g==null)return;hum.volume=g.state.generator?(g.boosted?.28f:.12f):0;hum.pitch=g.boosted?1.3f:1;}
 public static void Beep(float frequency,float duration){if(effects==null)return;int n=Mathf.CeilToInt(22050*duration);float[] a=new float[n];for(int i=0;i<n;i++)a[i]=Mathf.Sin(2*Mathf.PI*frequency*i/22050)*.16f*(1f-(float)i/n);var c=AudioClip.Create("relay",n,1,22050,false);c.SetData(a,0);effects.PlayOneShot(c);Object.Destroy(c,duration+1);}
 public static void Step(){if(effects!=null)effects.PlayOneShot(Resources.Load<AudioClip>("step"),.3f);}
 public static void Knock(){if(effects!=null)effects.PlayOneShot(Resources.Load<AudioClip>("knock"),.7f);}
}
}
