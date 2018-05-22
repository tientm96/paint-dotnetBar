using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Paint
{
    
    public partial class PaintForm : DevComponents.DotNetBar.Office2007RibbonForm
    {
        //Khai báo biến 
        enum item
        { Pen, Line, Oval, Rectangle,TriCan,TriVuong,Diamond,Select,Eraser,ColorPicker }
        enum fillType
        { NoFill, Solid, PathGradient, LinearGradient };
        fillType currentFillItem = fillType.NoFill; 
        item currentItem = item.Pen;
        Color currentColor = Color.Black;
        Color currentColor2 = Color.White;
        int currentWidth = 3;

        Pen pen;

        bool drawMouse = false;
        int x, y, x1, y1;
        int _x, _x1, _y, _y1;//lưu để dùng khi fill
        bool leftMouseButton; //để dùng khi fill
        bool exit = false;
        Graphics g;//current
        Bitmap drawing;//current
        Bitmap selectImage;
        Rectangle selectedArea;
        bool dragNdrop=false;

        FileInfo[] recentFile = new FileInfo[3];
        //list for pen
        List<Point> pointList;
        //list for eraser
        List<Rectangle> rectangleList;
        //list chứa 10 phần tử 
        ListExtra<Bitmap> drawingList = new ListExtra<Bitmap>(11);
        
        public PaintForm()
        {
            InitializeComponent();
            //this.Paint += new System.Windows.Forms.PaintEventHandler(pen);
            //set tooltips cho button
            setTooltips();
            drawing = new Bitmap(pictureBox.Width, pictureBox.Height, pictureBox.CreateGraphics());
            g = Graphics.FromImage(drawing);
            g.Clear(Color.White);

            drawingList.Add(drawing); //drawinglist[0] drawing trắng

            //drawingList.Last().Save("D:\\d.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            pictureBox.Image = drawing;
            drawing = new Bitmap(pictureBox.Width, pictureBox.Height);//drawing 1
            g = Graphics.FromImage(drawing);


            //chọn màu cho 2 ẻm colorPicker
            colorPickerDropDown1.SelectedColor = Color.Black;
            colorPickerDropDown2.SelectedColor = Color.White;
        }
        private void PaintForm_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// tooltips cho button
        /// </summary>
        private void setTooltips()
        {

            saveItem.Tooltip = "Save(Ctrl+S) \\nSave the current picture.";
            undoItem.Tooltip = "Undo (Ctrl+Z)\\nUndo last action.";
            redoItem.Tooltip = "Redo (Ctrl+Y)\\n Redo last action.";
            pasteItem.Tooltip = "Paste (Ctrl+V)\\nPaste the content of the Clipboard.";
            cutItem.Tooltip = "Cut (Ctrl+X)\\nCut the selection from the canvas\nand put it on the Clipboard.";
            copyItem.Tooltip = "Copy (Ctrl+C)\\nCopy the selection form the canvas\n and put it on the Clipboard. ";
            selectItem.Tooltip = "Selection\\nSellect a part of picture.";

            pencilItem.Tooltip = "Pencil\\nDraw a free-form";
            eraserItem.Tooltip = "Eraser\\nEraser part of the picture";
            colorPickerItem.Tooltip = "Color picker\\nPicker a color from the picture";

            fillLinearItem.Tooltip = "Fill Linear\\nFill the shape with Linear Gradient Color";
            fillPathItem.Tooltip = "Fill Path\\nFill the shape with Path Gradient Color";
            fillSolidItem.Tooltip = "Fill Solid\\nFill the shape with Solid Color";

            lineItem.Tooltip = "Line";
            ovalItem.Tooltip = "Oval";
            rectangleItem.Tooltip = "Rectangle";
            tricanItem.Tooltip = "Triagle";
            trivuongItem.Tooltip = "Right triagle";
            diamondItem.Tooltip = "Diamond";
            sizeItem.Tooltip = "Size\\nSelect the width for the selected tools.";
            colorPickerDropDown1.Tooltip = "Color 1 (foreground color)";
            colorPickerDropDown2.Tooltip = "Color 2 (background color)";
        }

        //MOUSE HANDLE
        private void pictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            drawMouse = true;
            x = e.X;
            y = e.Y;



            if (currentItem == item.Pen)
            {
                Console.WriteLine("x" + x + "y" + y);
                Console.WriteLine("x1" + x1 + "y1" + y1);
                Pen p = choosePen(e);



                // khởi tạo list tất cả các điểm để vẽ pen
                pointList = new List<Point>();
                pointList.Add(new Point(x, y)); // add điểm đầu tiên
                g.DrawLine(p, x, y, x + 1, y);

                pictureBox.Image = drawing;

            }
            if (currentItem == item.Eraser)
            {
                pointList = new List<Point>();
                SolidBrush brush = new SolidBrush(Color.White);

                rectangleList = new List<Rectangle>();
                rectangleList.Add(new Rectangle(new Point(x, y), new Size(currentWidth, currentWidth)));
                g.FillRectangles(brush,rectangleList.ToArray());
              
                pictureBox.Image = drawing;
            }
            if (currentItem==item.Select)
                //kích chuột trong vùng đã chọn->kéo thả
                if (x >= selectedArea.X && x <= selectedArea.X + selectedArea.Width
                             && y >= selectedArea.Y && y <= selectedArea.Y + selectedArea.Height)
                {
                    Console.Write("drag" + selectedArea.X + "   " + selectedArea.Y);
                    dragNdrop = true;

                }
                else //kích chuột ngoài
                {
                    //reset selectedArea-> khi click chuột ngoài rồi click vào vùng chọn không kéo thả được nữa
                    selectedArea.Width = 0;
                    selectedArea.Height = 0;
                    //xoá vùng chọn và không cho cut copy
                    drawing = new Bitmap(drawingList.ElementAt(drawingList.currentIndex));
                    //pictureBox.Image = drawingList.Last();
                    g = Graphics.FromImage(drawing);
                    pictureBox.Image = drawing;
                    cutItem.Enabled = false;
                    copyItem.Enabled = false;
                }

               
        }
        private void pictureBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //trỏ chuột về lại mặc định
            this.pictureBox.Cursor = Cursors.Cross;
            drawMouse = false;
            x1 = e.X; //lưu toạ độ lại
            y1 = e.Y; 
            Console.WriteLine(x1 +"    "+ y1);
            if(x!=x1&&y!=y1)
            {
                _x = x;
                _x1 = x1;
                _y = y;
                _y1 = y1;
                leftMouseButton = (e.Button == System.Windows.Forms.MouseButtons.Left);
            }

            if(currentItem==item.ColorPicker)
            {
                Color colorpicker = drawing.GetPixel(e.X, e.Y);


                if (e.Button == System.Windows.Forms.MouseButtons.Left)//chuột trái
                              colorPickerDropDown1.SelectedColor = currentColor = colorpicker;
                else
                    colorPickerDropDown1.SelectedColor = currentColor2 = colorpicker;
                goto jm;
            }
            if(currentItem==item.Select)
            {
                if (x1 - x == 0 || y1 - y == 0)//nếu chọn không được gì -> không làm gì cả
                    goto jump;
                    


                    //cho phép cut, copy 
                    cutItem.Enabled = true;
                    copyItem.Enabled = true;
                    
                    
                    if (dragNdrop == false)// đang chọn, không phải kéo thả
                    {
                        //lưu rectangle
                        selectedArea = Rectangle1(x, y, x1, y1);
                        //lưu ảnh
                        saveSelectedArea();
                       
                    }
                    else//đang kéo thả
                    {
                        
                        
                        selectedArea.X += (x1 - x);
                        selectedArea.Y += (y1 - y);
                        dragNdrop = false;
                        
                        //lấy hình ảnh k bị nét đứt
                        //drawing = new Bitmap(drawingList[drawingList.currentIndex]);
                        drawingList.AddBaseCapacity(drawing);
                        drawing = new Bitmap(drawingList.Last());
                        g = Graphics.FromImage(drawing);
                        dashDraw(selectedArea);
                        pictureBox.Image = drawing;
                        //dashDraw(new Rectangle(selectedArea.X + (x1 - x), selectedArea.Y + (y1 - y), selectedArea.Width - 1, selectedArea.Height));
                    }
                jump: ;
                
            }
            else // nếu select thì không lưu hình
                drawingList.AddBaseCapacity(drawing);
            
            




            //tạo drawing mới để vẽ 
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);

            jm:;
            
            Console.WriteLine(drawingList.currentIndex);

            //lưu
            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\");
            //System.IO.Directory.CreateDirectory(path);

            //for (int i = 0; i < drawingList.Count; i++)
            //    drawingList[i].Save(path + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        private void pictureBox_MouseMove(Object sender, MouseEventArgs e)
        {
            if (dragNdrop)
                this.pictureBox.Cursor = Cursors.SizeAll;
            else 
            this.pictureBox.Cursor = Cursors.Cross;

            if (drawMouse == false)
                goto jump;// k phải đang dí chuột thì nhảy tới jump
            x1 = e.X;
            y1 = e.Y;

            //g.Clear(Color.White);
            drawing = new Bitmap(drawingList.ElementAt(drawingList.currentIndex));
            //pictureBox.Image = drawingList.Last();
            g = Graphics.FromImage(drawing);
            
            //chọn pen
            Pen pen = choosePen(e);
            if (currentFillItem != fillType.NoFill)
                //vẽ phía trong
            {
                Brush brush = chooseBrush(e);
                brushShape(brush, x, y, x1, y1);
            }
            
            //Vẽ đường ngoài
            switch (currentItem)
            {
                case item.Select:
                    {
                        if(dragNdrop)
                        {
                            Console.WriteLine("Selecting");
                            //this.pictureBox.Cursor = Cursors.SizeAll;
                       
                          
                            //tạo nét đứt xung quanh
                            //dashDraw(new Rectangle(selectedArea.X+ (x1 - x), selectedArea.Y + (y1 - y),selectedArea.Width-1,selectedArea.Height));
                            //xoá hình select trên hình hiện tại
                            Bitmap nullImage = new Bitmap(selectImage);
                            Graphics nullGraphics = Graphics.FromImage(nullImage);
                            nullGraphics.Clear(Color.White);
                            g.DrawImageUnscaled(nullImage, selectedArea.X, selectedArea.Y);

                            //vẽ hình select lên vị trí mới
                            g.DrawImageUnscaled(selectImage, selectedArea.X + (x1 - x), selectedArea.Y + (y1 - y));
                            //pictureBox.Image = drawing;
                         
                        }
                        else
                        {
                            dashDraw(Rectangle1(x,y,x1,y1));
                        }
                        break;
                    }

                case item.Pen:
                    {
                        //Pen pen = pen_properties(currentColor, currentWidth);

                        //g.DrawLine(p, x, y, x1, y1);
                        //x = x1; y = y1;
                        pointList.Add(new Point(x1, y1));

                        try
                        {
                            g.DrawLines(pen, pointList.ToArray());
                            //g.DrawCurve(pen, pointList.ToArray());
                            ////g.DrawBeziers(pen, pointList.ToArray());
                        }
                        catch (Exception ex)
                        {

                        }

                        break;
                    }
                case item.Eraser:
                    {
                        SolidBrush brush = new SolidBrush(Color.White);
                        rectangleList.Add(new Rectangle(new Point(x1, y1), new Size(currentWidth, currentWidth)));
                        g.FillRectangles(brush, rectangleList.ToArray());
                        break;
                    }
                case item.Line:
                    {
                        g.DrawLine(pen, new Point(x, y), new Point(x1, y1));
                        //pictureBox1.CreateGraphics().DrawImageUnscaled(drawing, 0, 0);
                        break;
                    }
                case item.Oval:
                    {
                        g.DrawEllipse(pen, x, y, x1 - x, y1 - y);
                        break;
                    }
                case item.Rectangle:
                    {
                        g.DrawRectangle(pen,Rectangle1(x,y,x1,y1));
                        break;
                    }
                case item.TriCan:
                    {
                        Point[] pList = new Point[3];
                        pList[0]=new Point((x+x1)/2,y);
                        pList[1]=new Point(x,y1);
                        pList[2]=new Point(x1,y1);
                        g.DrawPolygon(pen,pList);
                        break;
                    }
                case item.TriVuong:
                    {
                        Point[] pList = new Point[3];
                        pList[0] = new Point(x,y);
                        pList[1] = new Point(x, y1);
                        pList[2] = new Point(x1, y1);
                        g.DrawPolygon(pen, pList);
                        break;
                    }
                case item.Diamond:
                    {
                        Point[] pList = new Point[4];
                        pList[0] = new Point((x + x1) / 2, y);
                        pList[1] = new Point(x, (y + y1) / 2);
                        pList[2] = new Point((x + x1) / 2, y1);
                        pList[3] = new Point(x1, (y + y1) / 2);
                        g.DrawPolygon(pen, pList);
                        break;
                    }
            }
            pictureBox.Image = drawing; // hiển thị
        jump: ;
            
        }

        //IMAGE EDITOR
        private void selectItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Select;
            unhightlightItem();
            selectItem.Checked = true;
        }
        private void cutItem_Click(object sender, EventArgs e)
        {

            Bitmap nullImage = new Bitmap(selectImage);
            Graphics nullGraphics = Graphics.FromImage(nullImage);
            nullGraphics.Clear(Color.White);
            //vẽ hình select lên vị trí trong hình drawing hiện tại
            drawing = new Bitmap(drawingList.ElementAt(drawingList.currentIndex));
            g = Graphics.FromImage(drawing);
            g.DrawImageUnscaled(nullImage, selectedArea);
            Console.WriteLine(drawingList.currentIndex);
            pictureBox.Image = drawing;
            drawingList.AddBaseCapacity(drawing);


            pasteItem.Enabled = true;
        }
        private void copyItem_Click(object sender, EventArgs e)
        {
            //drawing = new Bitmap(drawingList.ElementAt(drawingList.currentIndex));
            //g = Graphics.FromImage(drawing);
            pasteItem.Enabled = true;
        }
        private void pasteItem_Click(object sender, EventArgs e)
        {
            g.DrawImageUnscaled(selectImage, 0, 0);
            //pictureBox.Image = drawing;

            drawingList.AddBaseCapacity(drawing);
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);
            //cập nhật select thành toạ độ mới paste
            selectedArea = Rectangle1(0, 0, selectImage.Width, selectImage.Height);
            //tạo nét đứt xung quanh
            dashDraw(selectedArea);

            //g = Graphics.FromImage(drawing);
            pictureBox.Image = drawing;

        }
        /// <summary>
        /// Draw Dashdotline roundly the Rectangle
        /// </summary>
        private void dashDraw(Rectangle rec)
        {
            float[] dashValues = { 5, 2 };
            Pen pen = new Pen(Color.Blue, 1);
            pen.DashPattern = dashValues;
            Rectangle rec1 = new Rectangle(rec.X - 1, rec.Y - 1, rec.Width + 1, rec.Height + 1);
            g.DrawRectangle(pen, rec1);
        }
        /// <summary>
        /// lưu vị trí select trong clipboard
        /// </summary>
        private void saveSelectedArea()
        {
            //lưu hình select
            selectImage = new Bitmap(selectedArea.Width, selectedArea.Height);
            using (Graphics gSelect = Graphics.FromImage(selectImage))
            {
                gSelect.Clear(Color.White);

                //Rectangle source_rectangle = Rectangle1(x, y, x1, y1);
                Rectangle dest_rectangle = new Rectangle(0, 0, selectedArea.Width, selectedArea.Height);
                gSelect.DrawImage(drawingList.ElementAt(drawingList.currentIndex), dest_rectangle,
                    selectedArea, GraphicsUnit.Pixel);

            }
        }

        /// <summary>
        /// Brush the Shape
        /// </summary>
        private void brushShape(Brush brush, int x, int y, int x1, int y1)
        {
            switch (currentItem)
            {
                case item.Oval:
                    g.FillEllipse(brush, x, y, x1 - x, y1 - y);
                    break;
                case item.Rectangle:
                    g.FillRectangle(brush, Rectangle1(x, y, x1, y1));
                    break;
                case item.TriCan:
                    Point[] pList = new Point[3];
                    pList[0] = new Point((x + x1) / 2, y);
                    pList[1] = new Point(x, y1);
                    pList[2] = new Point(x1, y1);
                    g.FillPolygon(brush, pList);
                    break;
                case item.TriVuong:
                    pList = new Point[3];
                    pList[0] = new Point(x, y);
                    pList[1] = new Point(x, y1);
                    pList[2] = new Point(x1, y1);
                    g.FillPolygon(brush, pList);
                    break;
                case item.Diamond:
                    pList = new Point[4];
                    pList[0] = new Point((x + x1) / 2, y);
                    pList[1] = new Point(x, (y + y1) / 2);
                    pList[2] = new Point((x + x1) / 2, y1);
                    pList[3] = new Point(x1, (y + y1) / 2);
                    g.FillPolygon(brush, pList);
                    break;
            }
        }
        public Pen choosePen(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                pen = new Pen(currentColor, currentWidth);
            else //chuột phải=> màu 2
                pen = new Pen(currentColor2, currentWidth);
            
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            return pen;
        }
        private Brush chooseBrush(MouseEventArgs e)
        {
            Color primaryColor,subColor;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                primaryColor = currentColor2;
                subColor = currentColor;
            }
            else //chuột phải=> ngược lại
            {
                primaryColor = currentColor;
                subColor = currentColor2;
            }
            Brush brush = new SolidBrush(primaryColor); // default solid
            if (currentFillItem == fillType.PathGradient)
            {



                Point[] pList = new Point[5];
                pList[0] = new Point(x, y);
                pList[1] = new Point(x, y1);
                pList[2] = new Point(x1, y1);
                pList[3] = new Point(x1, y);
                //pList[4] = new Point(x, y);
                //pList[4] = new Point(x + 1, y);
                //pList[4] = new Point((x * 2 + x1 * 2) / 5, (y * 2 + y1 * 2) / 5);

                PathGradientBrush pthGrBrush = new PathGradientBrush(pList);

                //RectangleF rec = new RectangleF(x, y, x1 - x, y1 - y);
                pthGrBrush.CenterPoint = new PointF((x + x1) / 2, (y + y1) / 2);

                pthGrBrush.CenterColor = subColor;
                Color[] colors = { primaryColor };
                pthGrBrush.SurroundColors = colors;
                brush = pthGrBrush;
                
            }
            else if(currentFillItem == fillType.LinearGradient)
            {
                LinearGradientBrush linGrBrush = new LinearGradientBrush(
                new Point(x, y),
                new Point(x1, y1),
                primaryColor, subColor);
                brush = linGrBrush;
            }
            return brush;
        }
      

        //TOOLS
        private void pencilItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Pen;
            unhightlightItem();
            pencilItem.Checked = true;
            
            

            
        }
        private void eraserItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Eraser;
            unhightlightItem();
            eraserItem.Checked = true;
        }
        private void colorPickerItem_Click(object sender, EventArgs e)
        {
            currentItem = item.ColorPicker;
            unhightlightItem();
            colorPickerItem.Checked = true;
        }
        //FILL TOOLS
        private void fillSolidItem_Click(object sender, EventArgs e)
        {
            Brush brush;
            //=new SolidBrush(Color.White);
            //bỏ chọn tất cả button Fill
            uncheckedAllFill();
            if (currentFillItem == fillType.Solid)// đang là Solid
            {
                currentFillItem = fillType.NoFill;
                brush = new SolidBrush(Color.White);
            }

            else    //không là solid
            {//đổi
                fillSolidItem.Checked = true;
                currentFillItem = fillType.Solid;

                Color primaryColor, subColor;
                if (leftMouseButton == true)
                {
                    primaryColor = currentColor2;
                    subColor = currentColor;

                }
                else //chuột phải=> ngược lại
                {
                    primaryColor = currentColor;
                    subColor = currentColor2;

                }
                brush = new SolidBrush(primaryColor);
            }
            //tô màu 


            if (_x == 0 && _x1 == 0 && _y == 0 && _y1 == 0)
                return;//bỏ vẽ
            brushShape(brush, _x, _y, _x1, _y1);

            pictureBox.Image = drawing;
            drawingList.AddBaseCapacity(drawing);

            //tạo drawing mới để vẽ 
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);



        }
        private void fillPathItem_Click(object sender, EventArgs e)
        {
            Brush brush;


            //bỏ chọn tất cả button Fill
            uncheckedAllFill();
            if (currentFillItem == fillType.PathGradient)// đang là Path
            {
                currentFillItem = fillType.NoFill;

                brush = new SolidBrush(Color.White);
            }

            else //đổi
            {
                fillPathItem.Checked = true;
                currentFillItem = fillType.PathGradient;

                //đổi brush
                Color primaryColor, subColor;
                //set màu dựa trên chuột trái phải
                if (leftMouseButton == true)
                {
                    primaryColor = currentColor2;
                    subColor = currentColor;
                }
                else //chuột phải=> ngược lại
                {
                    primaryColor = currentColor;
                    subColor = currentColor2;
                }
                Point[] pList = new Point[5];
                pList[0] = new Point(x, y);
                pList[1] = new Point(x, y1);
                pList[2] = new Point(x1, y1);
                pList[3] = new Point(x1, y);
                //pList[4] = new Point(x, y);
                //pList[4] = new Point(x + 1, y);
                //pList[4] = new Point((x * 2 + x1 * 2) / 5, (y * 2 + y1 * 2) / 5);

                PathGradientBrush pthGrBrush = new PathGradientBrush(pList);

                //RectangleF rec = new RectangleF(x, y, x1 - x, y1 - y);
                pthGrBrush.CenterPoint = new PointF((x + x1) / 2, (y + y1) / 2);

                pthGrBrush.CenterColor = subColor;
                Color[] colors = { primaryColor };
                pthGrBrush.SurroundColors = colors;
                brush = pthGrBrush;
            }




            //vẽ 
            if (_x == 0 && _x1 == 0 && _y == 0 && _y1 == 0)
                return;//bỏ vẽ
            brushShape(brush, _x, _y, _x1, _y1);


            pictureBox.Image = drawing;
            drawingList.AddBaseCapacity(drawing);

            //tạo drawing mới để vẽ 
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);

        }
        private void fillLinearItem_Click(object sender, EventArgs e)
        {
            Brush brush;
            //bỏ chọn tất cả button Fill
            uncheckedAllFill();
            if (currentFillItem == fillType.LinearGradient)// đang là Linear=> nofill
            {
                currentFillItem = fillType.NoFill;
                //set brush
                brush = new SolidBrush(Color.White);
            }
            else //đổi
            {
                fillLinearItem.Checked = true;
                currentFillItem = fillType.LinearGradient;
                //set brush
                Color primaryColor, subColor;
                //set màu dựa trên chuột trái phải
                if (leftMouseButton == true)
                {
                    primaryColor = currentColor2;
                    subColor = currentColor;
                }
                else //chuột phải=> ngược lại
                {
                    primaryColor = currentColor;
                    subColor = currentColor2;
                }
                brush = new LinearGradientBrush(new Point(x, y),
                          new Point(x1, y1),
                          currentColor, currentColor2);

            }


            //         //vẽ 
            if (_x == 0 && _x1 == 0 && _y == 0 && _y1 == 0)
                return;//bỏ vẽ
            brushShape(brush, _x, _y, _x1, _y1);

            pictureBox.Image = drawing;
            drawingList.AddBaseCapacity(drawing);

            //tạo drawing mới để vẽ 
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);
        }


        //SHAPES
        private void ovalItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Oval;
            unhightlightItem();

            enableFill();//hiện item tô màu
            ovalItem.Checked = true;

        }
        private void lineItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Line;
            unhightlightItem();
            lineItem.Checked = true;
        }     
        private void rectangleItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Rectangle;
            unhightlightItem();
            enableFill();
            rectangleItem.Checked = true;
           
        }
        private void tricanItem_Click(object sender, EventArgs e)
        {
            currentItem = item.TriCan;
            unhightlightItem();
            enableFill();
            tricanItem.Checked = true;
        }
        private void trivuongItem_Click(object sender, EventArgs e)
        {
            currentItem = item.TriVuong;
            unhightlightItem();
            enableFill();
            trivuongItem.Checked = true;
        }
        private void diamondItem_Click(object sender, EventArgs e)
        {
            currentItem = item.Diamond;
            unhightlightItem();
            enableFill();
            diamondItem.Checked = true;
        }

        
        //SIZE
        private void sizeItem1_Click(object sender, EventArgs e)
        {
            currentWidth = 1;
            sizeItem.Text = "1 px";
        }
        private void sizeItem2_Click(object sender, EventArgs e)
        {
            currentWidth = 3;
            sizeItem.Text = "3 px";
        }
        private void sizeItem3_Click(object sender, EventArgs e)
        {
            currentWidth = 5;
            sizeItem.Text = "5 px";
        }
        private void sizeItem4_Click(object sender, EventArgs e)
        {
            currentWidth = 8;
            sizeItem.Text = "8 px";
        }


        //COLOR HANDLE
        private void colorPickerDropDown1_Click(object sender, System.EventArgs e)
        {
            colorPickerDropDown1.DisplayMoreColorsDialog();
        }
        private void colorPickerDropDown2_Click(object sender, System.EventArgs e)
        {
            colorPickerDropDown2.DisplayMoreColorsDialog();
        }
        private void colorPickerDropDown1_SelectedColorChanged(object sender, EventArgs e)
        {
            currentColor = colorPickerDropDown1.SelectedColor;
        }
        private void colorPickerDropDown2_SelectedColorChanged(object sender, EventArgs e)
        {
            currentColor2 = colorPickerDropDown2.SelectedColor;
        }
     


        ///FILE MENU
        private void fileNewMenuItem_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs ex = new FormClosingEventArgs(CloseReason.UserClosing, false);
            closeUseDialog(ex);
            if (ex.Cancel == false)//không phải cancel
            {
                drawing = new Bitmap(pictureBox.Width, pictureBox.Height);
                g = Graphics.FromImage(drawing);
                g.Clear(Color.White);
                drawingList.AddBaseCapacity(drawing);
                pictureBox.Image = drawing;
            }

        }
        private void fileOpenMenuItem_Click(object sender, EventArgs e)
        {
            System.IO.Stream myStream;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "D:\\";
            openFileDialog1.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            drawing = new Bitmap(myStream);
                            drawingList.AddBaseCapacity(drawing);
                            pictureBox.Size = drawing.Size;
                            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                            pictureBox.Image = drawing;
                            //tạo mới để vẽ
                            drawing = new Bitmap(drawingList.Last());
                            g = Graphics.FromImage(drawing);
                            //đổi tên form
                            changeFormText(getNameFromStream(myStream));
                            addToRecentItem(myStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            //changeRecentFile();
        }
        private void fileSaveMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        private void fileInforItem_Click(object sender, EventArgs e)
        {
            InfoForm infoForm = new InfoForm();
            infoForm.StartPosition = FormStartPosition.CenterScreen;
            infoForm.Show(this);
        }
        private void fileCloseMenuItem_Click(object sender, EventArgs e)
        {
            FormClosingEventArgs ex = new FormClosingEventArgs(CloseReason.UserClosing, false);
            closeUseDialog(ex);
            if (ex.Cancel == false)
            {
                exit = true;// gắn cờ để khỏi hiện hộp thoại khi vào hàm Closing
                this.Close();
            }


        }
        private void recentItem1_Click(object sender, EventArgs e)
        {
            drawing = new Bitmap(recentItem1.Tooltip);
            drawingList.AddBaseCapacity(drawing);
            pictureBox.Size = drawing.Size;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.Image = drawing;
            //drawing.Save(  "r.png", System.Drawing.Imaging.ImageFormat.Png);
            //tạo mới để vẽ
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);
        }
        private void recentItem2_Click(object sender, EventArgs e)
        {
            drawing = new Bitmap(recentItem2.Tooltip);
            drawingList.AddBaseCapacity(drawing);
            pictureBox.Size = drawing.Size;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.Image = drawing;
            //drawing.Save(  "r.png", System.Drawing.Imaging.ImageFormat.Png);
            //tạo mới để vẽ
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);
        }
        private void recentItem3_Click(object sender, EventArgs e)
        {
            drawing = new Bitmap(recentItem3.Tooltip);
            drawingList.AddBaseCapacity(drawing);
            pictureBox.Size = drawing.Size;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.Image = drawing;
            //drawing.Save(  "r.png", System.Drawing.Imaging.ImageFormat.Png);
            //tạo mới để vẽ
            drawing = new Bitmap(drawingList.Last());
            g = Graphics.FromImage(drawing);
        }

        
        //FILE MENU IMPLEMENTS
        ///<summary>
        ///thêm file vào đầu danh sách recentItem
        ///</summary>
        private void addToRecentItem(Stream stream)
        {
            FileStream fs = stream as FileStream;
            if (recentItem2.Text != "")//recent2 đã có file thì dồn xuống recent3
            {
                recentItem3.Text = recentItem2.Text;
                recentItem3.Tooltip = recentItem2.Tooltip;
                recentItem3.Enabled = true;//cho phép dùng
            }
            if(recentItem1.Text!="")//recent1 đã có file thì dồn xuống recent2
            {
                recentItem2.Text = recentItem1.Text;
                recentItem2.Tooltip = recentItem1.Tooltip;
                recentItem2.Enabled = true;
            }
            //recent1
            //tooltip=> đường dẫn file
            recentItem1.Tooltip = fs.Name;
            //đổi text recentItem = tên file
            recentItem1.Text = getNameFromStream(stream);
            recentItem1.Enabled = true;

        }
        //lấy tên file
        private String getNameFromStream(Stream stream)
        {
            FileStream fs = stream as FileStream;
            String name = fs.Name;
            string[] separators = new string[] {"\\"};
            String[] sp;
            sp = name.Split(separators,StringSplitOptions.None);
            return sp.Last();
        }
        private void changeFormText(String name)
        {
            this.Text = name;
        }

        private void saveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp";
            saveFileDialog.Title = "Save an Image File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.  
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the  
                // File type selected in the dialog box.  
                // NOTE that the FilterIndex property is one-based.  
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        pictureBox.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Png);
                        break;


                    case 2:

                        pictureBox.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        pictureBox.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }
                //thay tên form hiển thị
                changeFormText(getNameFromStream(fs));
                //cập nhật recent File
                addToRecentItem(fs);
                fs.Close();
                //changeRecentFile();
            }
        }
        private void PaintForm_Closing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (exit == false) // chưa xác định exit 
                closeUseDialog(e);//thì hiện hộp thoại



        }
        private void closeUseDialog(FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Would you like to save your changes?",
             "Save",
             MessageBoxButtons.YesNoCancel,
             MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                saveFile();
                //this.OnClosing(e);
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else // NO
            {
                //this.OnClosing(e);
            }
        }



   

      

        
        //TASKBAR ITEM
        private void saveItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        private void undoItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine(drawingList.currentIndex);
            if (drawingList.currentIndex > 0)
            {
                drawing = new Bitmap(drawingList.ElementAt(--drawingList.currentIndex));
                g = Graphics.FromImage(drawing);
                pictureBox.Size = drawing.Size;
                pictureBox.Image = drawing;
            }

        }
        private void redoItem_Click(object sender, EventArgs e)
        {
            if (drawingList.currentIndex < drawingList.Count - 1)//hiện hành: trước hoặc là kế cuối 
            {                                                           // vẽ hình sau nó lên picturebox
                drawing = new Bitmap(drawingList.ElementAt(++drawingList.currentIndex));
                g = Graphics.FromImage(drawing);
                pictureBox.Size = drawing.Size;
                pictureBox.Image = drawing;
            }
        }


        //chọn 1 trong nhóm item
     


        
        //Hình chữ nhật không lo biến <>
        private Rectangle Rectangle1(int x, int y, int x1, int y1)
        {
            Rectangle rec = new Rectangle(Math.Min(x,x1),Math.Min(y,y1),Math.Abs(x1-x),Math.Abs(y1-y));

            return rec;
        }

        private void unhightlightItem()
        {
            pencilItem.Checked = false;

            selectItem.Checked = false;
            eraserItem.Checked = false;
            colorPickerItem.Checked = false;
            //magnifierItem.Checked = false;

            lineItem.Checked = false;
            ovalItem.Checked = false;
            rectangleItem.Checked = false;
            tricanItem.Checked = false;
            trivuongItem.Checked = false;
            diamondItem.Checked = false;


            disableFill();
            disableClipBoard();
        }
        private void enableFill()
        {
            fillSolidItem.Enabled = true;
            fillPathItem.Enabled = true;
            fillLinearItem.Enabled = true;
        }
        private void disableFill()
        {
            fillSolidItem.Enabled = false;
            fillPathItem.Enabled = false;
            fillLinearItem.Enabled = false;
        }
        private void uncheckedAllFill()
        {
            fillPathItem.Checked = false;
            fillSolidItem.Checked = false;
            fillLinearItem.Checked = false;
        }
        private void disableClipBoard()
        {
            cutItem.Enabled = false;
            copyItem.Enabled = false;
            pasteItem.Enabled = false;
        }




        //FORM HANDLE
        private void shortcutKey(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Console.Write("aa");
            if (e.Control)
                switch (e.KeyCode)
                {
                    case Keys.Z: //ctrl +z => undo
                        Console.WriteLine("hus");
                        undoItem_Click(this, e); // truyền đại vẫn ok
                        break;

                    case Keys.Y://redo
                        redoItem_Click(this, e);
                        break;
                    case Keys.N: //new
                        fileNewMenuItem_Click(this, e);
                        break;
                    case Keys.O: //open
                        fileOpenMenuItem_Click(this, e);
                        break;
                    case Keys.S: //save
                        saveFile();
                        break;

                    case Keys.X://cut
                        if (cutItem.Enabled == true)
                            cutItem_Click(this, e);
                        break;
                    case Keys.C: //copy
                        if (copyItem.Enabled == true) // nếu button Copy đang cho phép
                            copyItem_Click(this, e);
                        break;
                    case Keys.V: //copy
                        if (pasteItem.Enabled == true) // nếu button Copy đang cho phép
                            pasteItem_Click(this, e);
                        break;
                }


        }
        private void PaintForm_SizeChanged(object sender, System.EventArgs e)
        {
            panelEx1.Size = new Size(this.Width - 15, this.Height - 160);
        }

      
    }
}
