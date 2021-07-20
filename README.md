<h1><span style="color: #000000;"><strong>Combined Character Controller&nbsp;</strong></span></h1>
<p>An adaptive environment-based controller for multiple visual perspectives supporting advanced ground detection and multiple integrated gameplay tools.</p>
<p><img src="https://i.ibb.co/z87bWLq/ezgif-com-gif-maker.gif" alt="" width="499" height="281" /></p>
<h3>Advanced Ground Checker</h3>
<p><span style="font-weight: 300;">Solutions:</span></p>
<ul>
<li style="font-weight: 400;" aria-level="1"><span style="font-weight: 400;">Check sphere below the character&nbsp;</span></li>
<li style="font-weight: 400;" aria-level="1"><span style="font-weight: 400;">Sherecasting below the character</span></li>
<li style="font-weight: 400;" aria-level="1"><span style="font-weight: 400;">Calculating the normal of the surface and if the normals are not facing the same directing then we&rsquo;re colliding; with the addition of raycasting collision check.&nbsp;</span></li>
</ul>
<p><span style="font-weight: 400;"><img src="https://i.ibb.co/gtCWJ5Y/ezgif-com-gif-maker.gif" alt="" width="500" height="281" /></span></p>
<h3><strong>Head Bobbing and Movement Speed Interpolation </strong></h3>
<p><span style="font-weight: 300;">Key Elements and Integration:</span></p>
<ul>
<li style="font-weight: 300;" aria-level="1"><span style="font-weight: 300;">Rotation around a fixed point (pivot)&nbsp;</span></li>
<li style="font-weight: 300;" aria-level="1"><span style="font-weight: 300;">Screen facing the object independent of the rotation</span></li>
<li style="font-weight: 300;" aria-level="1"><span style="font-weight: 300;">If the object (character) in not moving rotate the object independendently around the object; else smoothly rotate the character to face the position of the viewpoint.</span></li>
</ul>
<p>&nbsp;</p>
