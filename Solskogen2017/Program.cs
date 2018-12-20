using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK;
using Solskogen2017.EFFECTS;
using Solskogen2017.GRAPHICS;

namespace Solskogen2017
{
    class Program
    {
        private enum RenderMode
        {
            WIREFRAME,
            PIXELBUFFER,
            DEPTH,
            DIFFUSE_TEX,
            NORMAL_TEX
        }
        static void Main(string[] args)
        {
            using (var game = new GameWindow())
            {
                game.Size = new System.Drawing.Size(1920, 1080);
                game.Location = new System.Drawing.Point(0, 0);
                GRAPHICS.GlDiag.PrintDiag();

                //Part currentPart = new VoxelizerPart();
                //Part currentPart = new TerrainPart();
                Part currentPart = new MagicaVoxelPart();

				RenderMode mode = RenderMode.PIXELBUFFER;
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.KeyDown += (sender, e) => {
                    if (e.Key == Key.Escape)
                    {
                        game.Exit();
                    }
                    else if (e.Key == Key.W)
                    {
                        mode = mode==RenderMode.WIREFRAME?RenderMode.PIXELBUFFER:RenderMode.WIREFRAME;
                    }
                    else if(e.Key == Key.M)
                    {
                        switch(mode)
                        {
                            case RenderMode.DEPTH:
                                mode = RenderMode.PIXELBUFFER;
                                break;
                            case RenderMode.PIXELBUFFER:
                                mode = RenderMode.WIREFRAME;
                                break;
                            case RenderMode.WIREFRAME:
                                mode = RenderMode.DIFFUSE_TEX;
                                break;
                            case RenderMode.DIFFUSE_TEX:
                                mode = RenderMode.NORMAL_TEX;
                                break;
                            case RenderMode.NORMAL_TEX:
                                mode = RenderMode.DEPTH;
                                break;
                        }
                    }
                };
                ShadowEffect shadow = new ShadowEffect(game);
                Context context = new GRAPHICS.Context((float)Math.PI / 2f, (float)game.Width / (float)game.Height, 0.01f, 1000f, shadow);
                context.CameraPosition = new Vector3(10f, 10f, 10f);

                Framebuffer diffuseBuffer = new Framebuffer(game);
                Framebuffer normalBuffer = new Framebuffer(game);
                

                GL.ClearColor(0f, 0f, 0.4f, 1f);
                
                double timer = 0f;
                game.RenderFrame += (sender, e) =>
                {
                    timer += e.Time;

                    //context.CameraPosition = new Vector3((float)Math.Sin(timer) * 5f + 50f, 50f+(float)Math.Sin(timer*0.2f)*5f, (float)timer* 10f);
                    //context.LookAt = new Vector3(50f, 50f, (float)timer*10f+5f);

                    context.CameraPosition = new Vector3((float)Math.Sin(timer) * 50f, 50f, (float)Math.Cos(timer) * 50f);
                    context.LookAt = new Vector3(0f, 20f, 0f);
                    //context.LightPos = new Vector3(50f-(float)Math.Sin(-timer*0.5f)*70f, 50f+(float)Math.Sin(timer*0.4f)*25f, 50f-(float)Math.Cos(-timer*0.5f)*70f);
                    context.LightLookAt = new Vector3(0f, 0f, 0f);
                    context.LightPos = new Vector3((float)Math.Sin(timer * -0.8f) * 50f, 50f, (float)Math.Cos(timer * -0.8f) * 50f);


                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    if (mode == RenderMode.WIREFRAME)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    }
                    else
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    }
                    currentPart.tick((float)e.Time, (float)timer);

                    //Shadow pass
                    shadow.BeginShadowRendering(context);
                    currentPart.render(context, Part.RenderPass.SHADOW, (float)e.Time, (float)timer);
                    shadow.EndShadowRendering(context);

                    //Diffuse pass
                    diffuseBuffer.BeginDraw();
                    currentPart.render(context, Part.RenderPass.DIFFUSE, (float)e.Time, (float)timer);
                    diffuseBuffer.EndDraw();

                    //Normal pass
                    normalBuffer.BeginDraw();
                    currentPart.render(context, Part.RenderPass.NORMAL, (float)e.Time, (float)timer);
                    normalBuffer.EndDraw();

                    if(mode == RenderMode.DEPTH)
                    {
                        GL.Disable(EnableCap.DepthTest);
                        shadow.ShadowTexture.Blit();
                        GL.Enable(EnableCap.DepthTest);
                    }
                    else if (mode == RenderMode.DIFFUSE_TEX)
                    {
                        GL.Disable(EnableCap.DepthTest);
                        diffuseBuffer.Texture.Blit();
                        GL.Enable(EnableCap.DepthTest);
                    }
                    else if (mode == RenderMode.NORMAL_TEX)
                    {
                        GL.Disable(EnableCap.DepthTest);
                        normalBuffer.Texture.Blit();
                        GL.Enable(EnableCap.DepthTest);
                    }
                    else
                    {
                        //Diffuse pass
                        currentPart.render(context, Part.RenderPass.UNK, (float)e.Time, (float)timer);
                    }

                    game.SwapBuffers();
                };

                game.Run(60f);

            }
        }
    }
}
